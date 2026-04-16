using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.E2E.Tests.Modules.Payments;

[Trait("Category", "E2E")]
[Trait("Module", "Payments")]
public class PaymentsEndToEndTests : IClassFixture<TestContainerFixture>, IAsyncLifetime
{
    private readonly TestContainerFixture _fixture;

    public PaymentsEndToEndTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        await _fixture.CleanupDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task CreateSubscription_Should_PersistPendingSubscription()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var providerId = Guid.NewGuid();
        var request = new
        {
            ProviderId = providerId,
            PlanId = "price_premium_monthly"
        };

        // Act
        var response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/payments/subscriptions", request, TestContainerFixture.JsonOptions);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"CreateSubscription failed with {response.StatusCode}. Error: {error}");
        }

        var content = await response.Content.ReadFromJsonAsync<CheckoutResponse>(TestContainerFixture.JsonOptions);
        content.Should().NotBeNull();
        content!.CheckoutUrl.Should().StartWith("https://checkout.stripe.com/mock_");

        // Verificar estado no banco de dados
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<PaymentsDbContext>();
            var subscription = await dbContext.Subscriptions.FirstOrDefaultAsync(s => s.ProviderId == providerId);
            
            subscription.Should().NotBeNull();
            subscription!.Status.Should().Be(ESubscriptionStatus.Pending);
        });
    }

    [Fact]
    public async Task StripeWebhook_CheckoutSessionCompleted_Should_PersistToInbox()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var providerId = Guid.NewGuid();
        
        // 1. Criar uma subscription pendente primeiro
        var createRequest = new { ProviderId = providerId, PlanId = "price_premium_monthly" };
        var createResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/payments/subscriptions", createRequest, TestContainerFixture.JsonOptions);
        createResponse.EnsureSuccessStatusCode();

        // 2. Preparar payload mock do Webhook Stripe com campos obrigatórios do Stripe.net
        // Nota: Usar JSON bruto com snake_case pois EventUtility.ParseEvent exige o formato Stripe nativo
        var externalSubId = "sub_stripe_12345";
        var webhookJson = $$"""
        {
            "id": "evt_test_123",
            "object": "event",
            "type": "checkout.session.completed",
            "api_version": "2024-06-20",
            "livemode": false,
            "created": {{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}},
            "data": {
                "object": {
                    "id": "cs_test_123",
                    "object": "checkout.session",
                    "subscription": "{{externalSubId}}",
                    "metadata": {
                        "provider_id": "{{providerId}}"
                    }
                }
            }
        }
        """;

        // Act - Enviar webhook
        var webhookContent = new StringContent(webhookJson, System.Text.Encoding.UTF8, "application/json");
        var webhookResponse = await _fixture.ApiClient.PostAsync("/api/v1/payments/webhooks/stripe", webhookContent);
        
        if (webhookResponse.StatusCode != HttpStatusCode.OK)
        {
            var errorBody = await webhookResponse.Content.ReadAsStringAsync();
            throw new Exception($"Webhook failed with {webhookResponse.StatusCode}. Error Body: {errorBody}");
        }

        // 3. Verificar que a mensagem foi salva na inbox (teste síncrono do endpoint)
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<PaymentsDbContext>();
            var inboxMessage = await dbContext.InboxMessages.AsNoTracking()
                .FirstOrDefaultAsync(m => m.Type == "checkout.session.completed");
            
            inboxMessage.Should().NotBeNull();
            inboxMessage!.Content.Should().Contain(externalSubId);
        });

        // 4. O processamento assíncrono pelo ProcessInboxJob acontece em background.
        // Em ambiente de testes E2E com containers, o job pode não executar ou ter delay.
        // Por isso, verificamos apenas que o endpoint grava corretamente na inbox.
        // A ativação da subscription via background job é coberta em outros testes de integração.
    }

    private record CheckoutResponse(string CheckoutUrl);
}
