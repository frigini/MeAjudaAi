using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Gateways;

public class StripePaymentGateway : IPaymentGateway
{
    private readonly string _apiKey;

    public StripePaymentGateway(IConfiguration configuration)
    {
        _apiKey = configuration["Stripe:ApiKey"] ?? throw new ArgumentNullException("Stripe:ApiKey is missing");
        StripeConfiguration.ApiKey = _apiKey;
    }

    public async Task<SubscriptionGatewayResult> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, CancellationToken cancellationToken)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = ["card"],
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Price = planId, // In Stripe, planId is often the Price ID
                        Quantity = 1,
                    },
                ],
                Mode = "subscription",
                SuccessUrl = "https://meajudaai.com.br/payments/success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "https://meajudaai.com.br/payments/cancel",
                Metadata = new Dictionary<string, string>
                {
                    { "provider_id", providerId.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

            return new SubscriptionGatewayResult(true, null, session.Url, null);
        }
        catch (StripeException ex)
        {
            return new SubscriptionGatewayResult(false, null, null, ex.Message);
        }
    }

    public async Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken)
    {
        try
        {
            var service = new SubscriptionService();
            await service.CancelAsync(externalSubscriptionId, cancellationToken: cancellationToken);
            return true;
        }
        catch (StripeException)
        {
            return false;
        }
    }
}
