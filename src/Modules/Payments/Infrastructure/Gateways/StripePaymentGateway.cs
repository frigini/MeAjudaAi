using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Shared.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Gateways;

public class StripePaymentGateway : IPaymentGateway
{
    private readonly RequestOptions _requestOptions;
    private readonly ILogger<StripePaymentGateway> _logger;
    private readonly string _successUrl;
    private readonly string _cancelUrl;

    public StripePaymentGateway(IConfiguration configuration, ILogger<StripePaymentGateway> logger)
    {
        var apiKey = configuration["Stripe:ApiKey"] ?? throw new ArgumentNullException("Stripe:ApiKey is missing");
        _requestOptions = new RequestOptions { ApiKey = apiKey };
        _logger = logger;
        _successUrl = configuration["Payments:SuccessUrl"] ?? "https://meajudaai.com.br/payments/success?session_id={CHECKOUT_SESSION_ID}";
        _cancelUrl = configuration["Payments:CancelUrl"] ?? "https://meajudaai.com.br/payments/cancel";
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
                        Price = planId, // No Stripe, planId geralmente é o ID do Price
                        Quantity = 1,
                    },
                ],
                Mode = "subscription",
                SuccessUrl = _successUrl,
                CancelUrl = _cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "provider_id", providerId.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options, _requestOptions, cancellationToken);

            return new SubscriptionGatewayResult(true, null, session.Url, null);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating subscription for Provider {ProviderId}", providerId);
            return new SubscriptionGatewayResult(false, null, null, ex.Message);
        }
    }

    public async Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken)
    {
        try
        {
            var service = new SubscriptionService();
            await service.CancelAsync(externalSubscriptionId, null, _requestOptions, cancellationToken);
            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error canceling subscription {ExternalId}", externalSubscriptionId);
            return false;
        }
    }
}
