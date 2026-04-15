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
        
        var successUrl = configuration["Payments:SuccessUrl"];
        var cancelUrl = configuration["Payments:CancelUrl"];
        
        if (string.IsNullOrWhiteSpace(successUrl))
            throw new ArgumentException("Payments:SuccessUrl configuration is missing. Cannot use production fallback.", "Payments:SuccessUrl");
        
        if (string.IsNullOrWhiteSpace(cancelUrl))
            throw new ArgumentException("Payments:CancelUrl configuration is missing. Cannot use production fallback.", "Payments:CancelUrl");
        
        _successUrl = successUrl;
        _cancelUrl = cancelUrl;
    }

    public async Task<SubscriptionGatewayResult> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, CancellationToken cancellationToken)
    {
        try
        {
            var priceService = new PriceService();
            var price = await priceService.GetAsync(planId, null, _requestOptions, cancellationToken);
            
            if (price.UnitAmount != (long)(amount.Amount * 100) || price.Currency != amount.Currency.ToLowerInvariant())
            {
                return new SubscriptionGatewayResult(false, null, null, $"Price mismatch: Stripe Price ({price.UnitAmount}, {price.Currency}) does not match provided amount ({amount.Amount * 100}, {amount.Currency})");
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = ["card"],
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Price = planId,
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

    public async Task<string?> CreateBillingPortalSessionAsync(string externalCustomerId, string returnUrl, CancellationToken cancellationToken)
    {
        try
        {
            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = externalCustomerId,
                ReturnUrl = returnUrl,
            };

            var service = new Stripe.BillingPortal.SessionService();
            var session = await service.CreateAsync(options, _requestOptions, cancellationToken);

            return session.Url;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating billing portal session for Customer {CustomerId}", externalCustomerId);
            return null;
        }
    }
}
