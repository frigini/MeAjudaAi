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

        // Verify database state
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
        
        // 1. Create a pending subscription first
        var createRequest = new { ProviderId = providerId, PlanId = "price_premium", Amount = 99.90m };
        var createResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/payments/subscriptions", createRequest, TestContainerFixture.JsonOptions);
        createResponse.EnsureSuccessStatusCode();

        // 2. Prepare mock Stripe Webhook payload with required Stripe.net fields
        var externalSubId = "sub_stripe_12345";
        var webhookPayload = new 
        { 
            id = "evt_test_123",
            @object = "event",
            type = "checkout.session.completed",
            api_version = "2024-06-20", // Added to prevent Stripe.net NullReferenceException
            created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            data = new 
            {
                @object = new 
                {
                    id = "cs_test_123",
                    @object = "checkout.session",
                    subscription = externalSubId,
                    metadata = new { provider_id = providerId.ToString() }
                }
            }
        };

        // Act - Send webhook
        var webhookResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/payments/webhooks/stripe", webhookPayload, TestContainerFixture.JsonOptions);
        
        if (webhookResponse.StatusCode != HttpStatusCode.OK)
        {
            var error = await webhookResponse.Content.ReadAsStringAsync();
            throw new Exception($"Webhook failed with {webhookResponse.StatusCode}. Error: {error}");
        }

        webhookResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Give the background worker a moment to process the Inbox
        await Task.Delay(2000); 

        // Assert - Verify subscription exists
        await _fixture.WithServiceScopeAsync(async services =>
        {
            var dbContext = services.GetRequiredService<PaymentsDbContext>();
            var subscription = await dbContext.Subscriptions.FirstOrDefaultAsync(s => s.ProviderId == providerId);
            
            subscription.Should().NotBeNull();
        });
    }

    private record CheckoutResponse(string CheckoutUrl);
}
