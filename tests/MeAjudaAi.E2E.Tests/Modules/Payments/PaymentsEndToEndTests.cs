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
            PlanId = "price_premium_monthly",
            Amount = 99.90m,
            Currency = "BRL"
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
    public async Task StripeWebhook_CheckoutSessionCompleted_Should_ActivateSubscription()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var providerId = Guid.NewGuid();
        
        // 1. Criar uma subscription pendente primeiro
        var createRequest = new { ProviderId = providerId, PlanId = "price_premium", Amount = 99.90m, Currency = "BRL" };
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
            var error = await webhookResponse.Content.ReadAsStringAsync();
            throw new Exception($"Webhook failed with {webhookResponse.StatusCode}. Error: {error}");
        }

        webhookResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Pollar até o worker processar o Inbox (máx. 10s)
        await WaitForConditionAsync(async () =>
        {
            using var scope = _fixture.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
            var sub = await dbContext.Subscriptions.AsNoTracking().FirstOrDefaultAsync(s => s.ProviderId == providerId);
            return sub?.Status == ESubscriptionStatus.Active;
        }, timeoutMs: 10000, intervalMs: 250);

        // Assert - Verificar que a subscription foi ativada com dados corretos
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<PaymentsDbContext>();
            var subscription = await dbContext.Subscriptions.AsNoTracking().FirstOrDefaultAsync(s => s.ProviderId == providerId);
            
            subscription.Should().NotBeNull();
            subscription!.Status.Should().Be(ESubscriptionStatus.Active);
            subscription.ExternalSubscriptionId.Should().Be(externalSubId);
        });
    }

    private record CheckoutResponse(string CheckoutUrl);

    private static async Task WaitForConditionAsync(Func<Task<bool>> condition, int timeoutMs = 10000, int intervalMs = 250)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        while (stopwatch.ElapsedMilliseconds < timeoutMs)
        {
            if (await condition()) return;
            await Task.Delay(intervalMs);
        }

        throw new TimeoutException($"Condition was not met within {timeoutMs}ms.");
    }
}
