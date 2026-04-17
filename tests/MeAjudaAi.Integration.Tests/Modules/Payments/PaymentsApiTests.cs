using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Modules.Payments.Domain.Repositories;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Modules.Payments;

public class PaymentsApiTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Payments | TestModule.Providers;
    private Guid _seededProviderId;

    private async Task SeedProviderAsync()
    {
        if (_seededProviderId != Guid.Empty) return;

        _seededProviderId = Guid.NewGuid();
        using var scope = Services.CreateScope();
        var providersDb = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        
        var contactInfo = new ContactInfo("test@company.com", "+5511999999999");
        var businessProfile = new BusinessProfile("Test Company", contactInfo, null);
        var provider = new Provider(_seededProviderId, Guid.NewGuid(), "Test Provider", EProviderType.Company, businessProfile);
        
        providersDb.Providers.Add(provider);
        await providersDb.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateSubscription_ShouldReturnCheckoutUrl()
    {
        // Arrange
        await SeedProviderAsync();
        var request = new
        {
            ProviderId = _seededProviderId,
            PlanId = "price_premium_monthly"
        };
        
        // Autentica como o dono do provider
        AuthConfig.ConfigureProvider("user-1", "user", _seededProviderId);

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
        var response = await Client.PostAsync("/api/payments/webhooks/stripe", content);

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
        await SeedProviderAsync();
        var providerId = _seededProviderId;
        
        // Setup: Autenticar como o dono do provider para passar no ownership check
        AuthConfig.ConfigureProvider("provider-id", "provider", providerId);

        // Setup: Criar uma assinatura ativa primeiro
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var sub = new Subscription(
            providerId, 
            "price_premium_monthly", 
            Money.FromDecimal(99.90m, "BRL"));
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
        AuthConfig.ConfigureProvider("provider-id", "provider", providerId);
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
        await SeedProviderAsync();
        var providerId = _seededProviderId;
        var otherProviderId = Guid.NewGuid();
        AuthConfig.ConfigureProvider("provider-id", "provider", otherProviderId); // Autenticado como provedor diferente
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
        await SeedProviderAsync();
        AuthConfig.ConfigureProvider("provider-id", "provider", _seededProviderId);
        
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
        await SeedProviderAsync();
        var request = new
        {
            ProviderId = _seededProviderId,
            PlanId = "invalid_plan"
        };
        
        AuthConfig.ConfigureProvider("user-1", "user", _seededProviderId);

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/payments/subscriptions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task StripeWebhook_CheckoutSessionMissingProviderId_ShouldEnqueueInboxMessage()
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
        var response = await Client.PostAsync("/api/payments/webhooks/stripe", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK); // Webhook endpoint sempre retorna OK se salvou na inbox
        
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var messages = await dbContext.InboxMessages.AsNoTracking().ToListAsync();
        var inboxMessage = messages.FirstOrDefault(m => m.Content.Contains("cs_missing_id"));
        inboxMessage.Should().NotBeNull();
    }

    [Fact]
    public async Task StripeWebhook_InvoicePaid_ShouldProcessSuccessfully_E2E()
    {
        // Arrange
        await SeedProviderAsync();
        var externalSubId = "sub_e2e_123";
        var providerId = _seededProviderId;
        
        using (var scope = Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
            var sub = new Subscription(providerId, "price_premium_monthly", Money.FromDecimal(99.90m, "BRL"));
            sub.Activate(externalSubId, "cus_e2e", DateTime.UtcNow.AddDays(1));
            dbContext.Subscriptions.Add(sub);
            await dbContext.SaveChangesAsync();
        }

        var webhookJson = $$$"""
        {
            "id": "evt_e2e_paid",
            "object": "event",
            "type": "invoice.paid",
            "api_version": "2026-03-25.dahlia",
            "data": {
                "object": {
                    "object": "invoice",
                    "id": "in_e2e_123",
                    "customer": "cus_e2e",
                    "amount_paid": 9990,
                    "currency": "brl",
                    "parent": {
                        "type": "subscription_details",
                        "subscription_details": {
                            "subscription": "{{{externalSubId}}}"
                        }
                    },
                    "lines": {
                        "data": [
                            {
                                "subscription": "{{{externalSubId}}}",
                                "period": {
                                    "end": 1800000000
                                }
                            }
                        ]
                    }
                }
            }
        }
        """;
        
        // Act - Enfileiramento via Webhook
        var content = new StringContent(webhookJson, System.Text.Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/api/payments/webhooks/stripe", content);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Disparo manual do processamento em background para o teste
        using (var scope = Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
            var subRepo = scope.ServiceProvider.GetRequiredService<ISubscriptionRepository>();
            var txRepo = scope.ServiceProvider.GetRequiredService<IPaymentTransactionRepository>();
            
            var messages = await dbContext.InboxMessages.Where(m => m.ExternalEventId == "evt_e2e_paid").ToListAsync();
            messages.Should().HaveCount(1);
            
            var job = ActivatorUtilities.CreateInstance<MeAjudaAi.Modules.Payments.Infrastructure.BackgroundJobs.ProcessInboxJob>(scope.ServiceProvider);
            var stripeEvent = Stripe.EventUtility.ParseEvent(messages[0].Content, throwOnApiVersionMismatch: false);
            var data = job.MapToStripeEventData(stripeEvent);
            
            await job.ProcessStripeEventAsync(data, subRepo, txRepo, CancellationToken.None);
            
            messages[0].MarkAsProcessed();
            await dbContext.SaveChangesAsync();
        }

        // Assert - Verificar efeitos no domínio
        using (var scope = Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
            var sub = await dbContext.Subscriptions.FirstAsync(s => s.ExternalSubscriptionId == externalSubId);
            
            // Período de expiração deve ter sido avançado para 2027-01-15 (timestamp 1800000000)
            sub.ExpiresAt.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1800000000).UtcDateTime);
            
            var transaction = await dbContext.PaymentTransactions.AnyAsync(t => t.ExternalTransactionId == "in_e2e_123");
            transaction.Should().BeTrue();
        }
    }

    [Fact]
    public async Task StripeWebhook_InvoicePaid_ShouldEnqueueInboxMessage()
    {
        // Arrange
        await SeedProviderAsync();
        var externalSubId = "sub_live_123";
        var providerId = _seededProviderId;
        
        // Setup: Criar uma assinatura ativa no DB
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var originalExpiresAt = DateTime.UtcNow.AddDays(1);
        var sub = new Subscription(
            providerId, 
            "price_premium_monthly", 
            Money.FromDecimal(99.90m, "BRL"));
        sub.Activate(externalSubId, "cus_123", originalExpiresAt);
        dbContext.Subscriptions.Add(sub);
        await dbContext.SaveChangesAsync();

        var webhookJson = $$$"""
        {
            "id": "evt_paid_123",
            "object": "event",
            "type": "invoice.paid",
            "api_version": "2026-03-25.dahlia",
            "data": {
                "object": {
                    "id": "in_123",
                    "object": "invoice",
                    "customer": "cus_123",
                    "amount_paid": 9990,
                    "currency": "brl",
                    "hosted_invoice_url": "https://stripe.com/invoice/123",
                    "parent": {
                        "type": "subscription_details",
                        "subscription_details": {
                            "subscription": "{{{externalSubId}}}"
                        }
                    },
                    "lines": {
                        "data": [
                            {
                                "subscription": "{{{externalSubId}}}",
                                "period": {
                                    "end": 1800000000
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
        var response = await Client.PostAsync("/api/payments/webhooks/stripe", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar se a mensagem foi gravada na Inbox
        var messages = await dbContext.InboxMessages.AsNoTracking().ToListAsync();
        var inboxMessage = messages.FirstOrDefault(m => m.Content.Contains("evt_paid_123"));
        inboxMessage.Should().NotBeNull();
        inboxMessage!.ProcessedAt.Should().BeNull(); // Estado inicial
    }

    [Fact]
    public async Task StripeWebhook_DuplicateEvent_ShouldReturnOk_AndNotProcessTwice()
    {
        // Arrange
        var externalEventId = "evt_duplicate_123";
        var webhookJson = $$$"""
        {
            "id": "{{{externalEventId}}}",
            "object": "event",
            "type": "customer.subscription.updated",
            "api_version": "2026-03-25.dahlia",
            "data": { "object": { "id": "sub_123", "object": "subscription" } }
        }
        """;
        var content = new StringContent(webhookJson, System.Text.Encoding.UTF8, "application/json");

        // Act - First call
        var response1 = await Client.PostAsync("/api/payments/webhooks/stripe", content);
        
        // Act - Second call (duplicate)
        var content2 = new StringContent(webhookJson, System.Text.Encoding.UTF8, "application/json");
        var response2 = await Client.PostAsync("/api/payments/webhooks/stripe", content2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var messages = await dbContext.InboxMessages.Where(m => m.ExternalEventId == externalEventId).ToListAsync();
        messages.Should().HaveCount(1); // Somente um registro deve existir
    }

    [Fact]
    public async Task StripeWebhook_EmptyBody_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.PostAsync("/api/payments/webhooks/stripe", new StringContent("", System.Text.Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetBillingPortal_WithUntrustedReturnUrl_ShouldReturnError()
    {
        // Arrange
        await SeedProviderAsync();
        var providerId = _seededProviderId;
        AuthConfig.ConfigureProvider("provider-id", "provider", providerId);

        // Setup: Criar uma assinatura ativa no DB
        using (var scope = Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
            var sub = new Subscription(providerId, "price_premium_monthly", Money.FromDecimal(99.90m, "BRL"));
            sub.Activate("sub_untrusted", "cus_untrusted", DateTime.UtcNow.AddMonths(1));
            dbContext.Subscriptions.Add(sub);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await Client.PostAsJsonAsync($"/api/v1/payments/subscriptions/billing-portal", new { providerId, returnUrl = "https://evil.com" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("não é confiável");
    }

    private record BillingPortalResponse(string PortalUrl);
    private record CheckoutResponse(string CheckoutUrl);
}
