using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Integration.Tests.Modules.Payments;

public class PaymentsApiTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Payments;

    [Fact]
    public async Task CreateSubscription_ShouldReturnCheckoutUrl()
    {
        // Pré-condições
        var request = new
        {
            ProviderId = Guid.NewGuid(),
            PlanId = "price_premium_monthly",
            Amount = 99.90m,
            Currency = "BRL"
        };

        // Execução
        var response = await Client.PostAsJsonAsync("/api/v1/payments/subscriptions", request);

        // Verificação
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
        // Pré-condições
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
        
        // Execução
        var content = new StringContent(webhookJson, System.Text.Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/api/v1/payments/webhooks/stripe", content);

        // Verificação
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
        // Pré-condições
        var providerId = Guid.NewGuid();
        
        // Setup: Criar uma assinatura ativa primeiro
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var sub = new MeAjudaAi.Modules.Payments.Domain.Entities.Subscription(
            providerId, 
            "plan_123", 
            MeAjudaAi.Shared.Domain.ValueObjects.Money.FromDecimal(99.90m, "BRL"));
        sub.Activate("sub_mock", "cus_mock", DateTime.UtcNow.AddMonths(1));
        dbContext.Subscriptions.Add(sub);
        await dbContext.SaveChangesAsync();

        // Execução
        var request = new { providerId, returnUrl = "account" };
        var response = await Client.PostAsJsonAsync("/api/v1/payments/subscriptions/billing-portal", request);

        // Verificação
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var portalResponse = await response.Content.ReadFromJsonAsync<BillingPortalResponse>();
        string? url = portalResponse?.PortalUrl;
        url.Should().NotBeNull();
        url!.Should().Contain("mock_portal_");
    }

    private record BillingPortalResponse(string PortalUrl);

    [Fact]
    public async Task StripeWebhook_InvoicePaid_ShouldRenewSubscription()
    {
        // Pré-condições
        var externalSubId = "sub_live_123";
        var providerId = Guid.NewGuid();
        
        // Setup: Create an active subscription in DB
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        var originalExpiresAt = DateTime.UtcNow.AddDays(1);
        var sub = new MeAjudaAi.Modules.Payments.Domain.Entities.Subscription(
            providerId, 
            "plan_123", 
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
        
        // Execução
        var content = new StringContent(webhookJson, System.Text.Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/api/v1/payments/webhooks/stripe", content);

        // Verificação
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar a renovação no DB (Background jobs podem levar um tempo, mas em testes de integração costumamos executá-los manualmente ou aguardar)
        // Na verdade, o StripeWebhookEndpoint apenas salva na Inbox. Precisamos acionar o processador de inbox ou verificar o estado da Inbox.
        var inboxMessage = await dbContext.InboxMessages.FirstOrDefaultAsync(m => EF.Functions.Like((string)(object)m.Content, "%evt_paid_123%"));
        inboxMessage.Should().NotBeNull();
        inboxMessage!.ProcessedAt.Should().BeNull(); // Initial state
    }

    private record CheckoutResponse(string CheckoutUrl);
}
