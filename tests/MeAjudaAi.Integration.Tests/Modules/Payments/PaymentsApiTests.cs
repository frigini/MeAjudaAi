using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Modules.Payments;

public class PaymentsApiTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Payments;

    [Fact]
    public async Task CreateSubscription_ShouldReturnCheckoutUrl()
    {
        // Arrange
        var request = new
        {
            ProviderId = Guid.NewGuid(),
            PlanId = "price_premium_monthly"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/payments/subscriptions", request);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Test failed with status {response.StatusCode}. Error: {error}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<CheckoutResponse>();
        content.Should().NotBeNull();
        content!.CheckoutUrl.Should().StartWith("https://checkout.stripe.com/mock_");
    }

    [Fact]
    public async Task StripeWebhook_ShouldReturnOk()
    {
        // Arrange
        var webhookJson = """
        {
            "id": "evt_123",
            "object": "event",
            "type": "checkout.session.completed",
            "api_version": "2024-06-20",
            "livemode": false,
            "created": 1712345678,
            "data": {
                "object": {
                    "id": "cs_test_123",
                    "object": "checkout.session",
                    "customer": "cus_123",
                    "subscription": "sub_123"
                }
            }
        }
        """;
        
        // Act
        var content = new StringContent(webhookJson, System.Text.Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/api/v1/payments/webhooks/stripe", content);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"Webhook failed with {response.StatusCode}. Error Body: {errorBody}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBillingPortal_ShouldReturnUrl()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        
        // Setup: Autenticar como o dono do provider para passar no ownership check
        this.AuthenticateAsProvider(providerId);

        // Setup: Criar uma assinatura ativa primeiro
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var sub = new MeAjudaAi.Modules.Payments.Domain.Entities.Subscription(
            providerId, 
            "price_premium_monthly", 
            MeAjudaAi.Shared.Domain.ValueObjects.Money.FromDecimal(99.90m, "BRL"));
        sub.Activate("sub_mock", "cus_mock", DateTime.UtcNow.AddMonths(1));
        dbContext.Subscriptions.Add(sub);
        await dbContext.SaveChangesAsync();

        // Act
        var request = new { providerId, returnUrl = "account" };
        var response = await Client.PostAsJsonAsync("/api/v1/payments/subscriptions/billing-portal", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var portalResponse = await response.Content.ReadFromJsonAsync<BillingPortalResponse>();
        string? url = portalResponse?.PortalUrl;
        url.Should().NotBeNull();
        url!.Should().Contain("mock_portal_");
    }

    [Fact]
    public async Task GetBillingPortal_WithNonExistentProvider_ShouldReturnNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        this.AuthenticateAsProvider(providerId);
        var request = new { providerId, returnUrl = "account" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/payments/subscriptions/billing-portal", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBillingPortal_ForOtherProvider_ShouldReturnForbidden()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var otherProviderId = Guid.NewGuid();
        this.AuthenticateAsProvider(otherProviderId); // Authenticated as different provider
        var request = new { providerId, returnUrl = "account" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/payments/subscriptions/billing-portal", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetBillingPortal_WithEmptyProviderId_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new { providerId = Guid.Empty, returnUrl = "account" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/payments/subscriptions/billing-portal", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateSubscription_WithInvalidPlan_ShouldReturnError()
    {
        // Arrange
        var request = new
        {
            ProviderId = Guid.NewGuid(),
            PlanId = "invalid_plan"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/payments/subscriptions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task StripeWebhook_CheckoutSessionMissingProviderId_ShouldReturnError()
    {
        // Arrange
        var webhookJson = $$$"""
        {
            "id": "evt_missing_id",
            "object": "event",
            "type": "checkout.session.completed",
            "data": {
                "object": {
                    "id": "cs_missing_id",
                    "metadata": { }
                }
            }
        }
        """;
        
        // Act
        var content = new StringContent(webhookJson, System.Text.Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/api/v1/payments/webhooks/stripe", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK); // Webhook endpoint always returns OK if saved to inbox
        
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var inboxMessage = await dbContext.InboxMessages.FirstOrDefaultAsync(m => EF.Functions.Like((string)(object)m.Content, "%cs_missing_id%"));
        inboxMessage.Should().NotBeNull();
    }

    private record BillingPortalResponse(string PortalUrl);

    [Fact]
    public async Task StripeWebhook_InvoicePaid_ShouldEnqueueInboxMessage()
    {
        // Arrange
        var externalSubId = "sub_live_123";
        var providerId = Guid.NewGuid();
        
        // Setup: Criar uma assinatura ativa no DB
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var originalExpiresAt = DateTime.UtcNow.AddDays(1);
        var sub = new MeAjudaAi.Modules.Payments.Domain.Entities.Subscription(
            providerId, 
            "price_premium_monthly", 
            MeAjudaAi.Shared.Domain.ValueObjects.Money.FromDecimal(99.90m, "BRL"));
        sub.Activate(externalSubId, "cus_123", originalExpiresAt);
        dbContext.Subscriptions.Add(sub);
        await dbContext.SaveChangesAsync();

        var webhookJson = $$$"""
        {
            "id": "evt_paid_123",
            "object": "event",
            "type": "invoice.paid",
            "data": {
                "object": {
                    "id": "in_123",
                    "subscription": "{{{externalSubId}}}",
                    "customer": "cus_123",
                    "amount_paid": 9990,
                    "currency": "brl",
                    "hosted_invoice_url": "https://stripe.com/invoice/123",
                    "lines": {
                        "data": [
                            {
                                "period": {
                                    "end": 1740000000
                                }
                            }
                        ]
                    }
                }
            }
        }
        """;
        
        // Act
        var content = new StringContent(webhookJson, System.Text.Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/api/v1/payments/webhooks/stripe", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar se a mensagem foi gravada na Inbox
        // Na verdade, o StripeWebhookEndpoint apenas salva na Inbox. A renovação real ocorre via ProcessInboxJob.
        var inboxMessage = await dbContext.InboxMessages.FirstOrDefaultAsync(m => EF.Functions.Like((string)(object)m.Content, "%evt_paid_123%"));
        inboxMessage.Should().NotBeNull();
        inboxMessage!.ProcessedAt.Should().BeNull(); // Estado inicial
    }

    private record CheckoutResponse(string CheckoutUrl);
}
