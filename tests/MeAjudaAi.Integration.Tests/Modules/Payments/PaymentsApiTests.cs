using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;

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
            PlanId = "price_premium_monthly",
            Amount = 99.90m,
            Currency = "BRL"
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
        // A more complete Stripe event payload
        var webhookPayload = new 
        { 
            id = "evt_123",
            @object = "event",
            type = "checkout.session.completed",
            api_version = "2024-06-20",
            created = 1712345678,
            data = new 
            {
                @object = new 
                {
                    id = "cs_test_123",
                    @object = "checkout.session",
                    customer = "cus_123",
                    subscription = "sub_123"
                }
            }
        };
        
        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/payments/webhooks/stripe", webhookPayload);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Webhook failed with {response.StatusCode}. Error: {error}");
        }

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private record CheckoutResponse(string CheckoutUrl);
}
