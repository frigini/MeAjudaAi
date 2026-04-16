using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Shared.Domain.ValueObjects;
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

        _successUrl = configuration["Payments:SuccessUrl"] ?? throw new ArgumentException("Payments:SuccessUrl is missing in configuration.");
        _cancelUrl = configuration["Payments:CancelUrl"] ?? throw new ArgumentException("Payments:CancelUrl is missing in configuration.");

        _requestOptions = new RequestOptions
        {
            ApiKey = apiKey
        };
    }

    public async Task<SubscriptionGatewayResult> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, CancellationToken cancellationToken)
    {
        if (IsZeroDecimalCurrency(amount.Currency) && amount.Amount % 1 != 0)
        {
            _logger.LogWarning("Attempt to create subscription with fractional amount for zero-decimal currency: {Currency} {Amount}", amount.Currency, amount.Amount);
            return new SubscriptionGatewayResult(false, null, null, $"Zero-decimal currency ({amount.Currency}) does not accept fractional amounts: {amount.Amount}");
        }

        try
        {
            var price = await _stripeService.GetPriceAsync(planId, _requestOptions, cancellationToken);
            
            var expectedAmount = ConvertToMinorUnits(amount.Currency, amount.Amount);
            if (price.UnitAmount != expectedAmount || price.Currency != amount.Currency.ToLowerInvariant())
            {
                _logger.LogError("Price mismatch detected. Stripe: {StripeAmount} {StripeCurrency}, Expected: {ExpectedAmount} {ExpectedCurrency}", 
                    price.UnitAmount, price.Currency, expectedAmount, amount.Currency);
                return new SubscriptionGatewayResult(false, null, null, "The plan amount or currency does not match the payment provider information.");
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
                SuccessUrl = _successUrl + "?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = _cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "provider_id", providerId.ToString() }
                }
            };

            var session = await _stripeService.CreateCheckoutSessionAsync(options, _requestOptions, cancellationToken);

            return new SubscriptionGatewayResult(true, null, session.Url, null);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating subscription for Provider {ProviderId}", providerId);
            return new SubscriptionGatewayResult(false, null, null, "Payment provider communication failure.");
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

    private static long ConvertToMinorUnits(string currency, decimal amount)
    {
        if (IsZeroDecimalCurrency(currency))
        {
            return (long)amount;
        }

        return (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);
    }

    private static bool IsZeroDecimalCurrency(string currency)
    {
        var zeroDecimalCurrencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "BIF", "CLP", "DJF", "GNF", "JPY", "KMF", "KRW", "MGA", "PYG", "RWF", "UGX", "VND", "VUV", "XAF", "XOF", "XPF"
        };

        return zeroDecimalCurrencies.Contains(currency);
    }
}
