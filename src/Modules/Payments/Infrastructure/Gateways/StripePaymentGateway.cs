using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Utilities;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Gateways;

public class StripePaymentGateway : IPaymentGateway
{
    private readonly ILogger<StripePaymentGateway> _logger;
    private readonly string _successUrl;
    private readonly string _cancelUrl;
    private readonly RequestOptions _requestOptions;
    private readonly IStripeService _stripeService;

    public StripePaymentGateway(
        IConfiguration configuration, 
        ILogger<StripePaymentGateway> logger,
        IStripeService stripeService)
    {
        _logger = logger;
        _stripeService = stripeService;
        var apiKey = configuration["Stripe:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new ArgumentException("Stripe API key is not configured. Please set Stripe:ApiKey in configuration.");
        }

        var clientBaseUrl = (configuration["ClientBaseUrl"] ?? "").TrimEnd('/');
        if (string.IsNullOrEmpty(clientBaseUrl))
        {
            throw new ArgumentException("ClientBaseUrl is missing in configuration.");
        }

        var successPath = configuration["Payments:SuccessUrl"] ?? throw new ArgumentException("Payments:SuccessUrl is missing in configuration.");
        var cancelPath = configuration["Payments:CancelUrl"] ?? throw new ArgumentException("Payments:CancelUrl is missing in configuration.");

        _successUrl = Uri.TryCreate(successPath, UriKind.Absolute, out var successUri) && 
                     (successUri.Scheme == Uri.UriSchemeHttp || successUri.Scheme == Uri.UriSchemeHttps)
                     ? successPath 
                     : $"{clientBaseUrl}/{successPath.TrimStart('/')}";

        _cancelUrl = Uri.TryCreate(cancelPath, UriKind.Absolute, out var cancelUri) && 
                    (cancelUri.Scheme == Uri.UriSchemeHttp || cancelUri.Scheme == Uri.UriSchemeHttps)
                    ? cancelPath 
                    : $"{clientBaseUrl}/{cancelPath.TrimStart('/')}";

        _requestOptions = new RequestOptions
        {
            ApiKey = apiKey
        };
    }

    public async Task<SubscriptionGatewayResult> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, CancellationToken cancellationToken)
    {
        if (CurrencyUtils.IsZeroDecimalCurrency(amount.Currency) && amount.Amount % 1 != 0)
        {
            _logger.LogWarning("Attempt to create subscription with fractional amount for zero-decimal currency: {Currency} {Amount}", amount.Currency, amount.Amount);
            return SubscriptionGatewayResult.Failed($"Zero-decimal currency ({amount.Currency}) does not accept fractional amounts: {amount.Amount}");
        }

        try
        {
            var price = await _stripeService.GetPriceAsync(planId, _requestOptions, cancellationToken);
            
            var expectedAmount = CurrencyUtils.ConvertToMinorUnits(amount.Amount, amount.Currency);
            if (price.UnitAmount != expectedAmount || !string.Equals(price.Currency, amount.Currency, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("Price mismatch detected. Stripe: {StripeAmount} {StripeCurrency}, Expected: {ExpectedAmount} {ExpectedCurrency}", 
                    price.UnitAmount, price.Currency, expectedAmount, amount.Currency);
                return SubscriptionGatewayResult.Failed("The plan amount or currency does not match the payment provider information.");
            }

            var successUrlWithSession = _successUrl.Contains('?') 
                ? $"{_successUrl}&session_id={{CHECKOUT_SESSION_ID}}" 
                : $"{_successUrl}?session_id={{CHECKOUT_SESSION_ID}}";

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
                SuccessUrl = successUrlWithSession,
                CancelUrl = _cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "provider_id", providerId.ToString() }
                }
            };

            var session = await _stripeService.CreateCheckoutSessionAsync(options, _requestOptions, cancellationToken);

            return SubscriptionGatewayResult.Succeeded(null, session.Url);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating subscription for Provider {ProviderId}", providerId);
            return SubscriptionGatewayResult.Failed("Payment provider communication failure.");
        }
    }

    public async Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _stripeService.CancelSubscriptionAsync(externalSubscriptionId, _requestOptions, cancellationToken);
            return result;
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

            return await _stripeService.CreateBillingPortalSessionAsync(options, _requestOptions, cancellationToken);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating billing portal session for Customer {CustomerId}", externalCustomerId);
            return null;
        }
    }
}
