using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Utilities;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Gateways;

public class StripePaymentGateway(
    IConfiguration configuration,
    ILogger<StripePaymentGateway> logger,
    IStripeService stripeService) : IPaymentGateway
{
    private readonly string _successUrl;
    private readonly string _cancelUrl;
    private readonly RequestOptions _requestOptions;

    public StripePaymentGateway(IConfiguration configuration, ILogger<StripePaymentGateway> logger)
        : this(configuration, logger, new StripeService())
    {
        var apiKey = configuration["Stripe:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new ArgumentException("Stripe ApiKey is missing in configuration.");
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

    public async Task<SubscriptionGatewayResult> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, CancellationToken cancellationToken, string? idempotencyKey = null)
    {
        if (CurrencyUtils.IsZeroDecimalCurrency(amount.Currency) && amount.Amount % 1 != 0)
        {
            logger.LogWarning("Attempt to create subscription with fractional amount for zero-decimal currency: {Currency} {Amount}", amount.Currency, amount.Amount);
            return SubscriptionGatewayResult.Failed($"Zero-decimal currency ({amount.Currency}) does not accept fractional amounts: {amount.Amount}");
        }

        try
        {
            // Resolve domain planId to actual Stripe Price ID via configuration mapping
            var stripePriceId = configuration[$"Payments:Plans:{planId}:StripePriceId"] ?? planId;

            var price = await stripeService.GetPriceAsync(stripePriceId, _requestOptions, cancellationToken);
            
            var expectedAmount = CurrencyUtils.ConvertToMinorUnits(amount.Amount, amount.Currency);
            if (price.UnitAmount != expectedAmount || !string.Equals(price.Currency, amount.Currency, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogError("Price mismatch detected. Stripe: {StripeAmount} {StripeCurrency}, Expected: {ExpectedAmount} {ExpectedCurrency}", 
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
                        Price = stripePriceId,
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

            var requestOptions = new RequestOptions { ApiKey = _requestOptions.ApiKey };
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                requestOptions.IdempotencyKey = idempotencyKey;
            }

            var session = await stripeService.CreateCheckoutSessionAsync(options, requestOptions, cancellationToken);

            return SubscriptionGatewayResult.Succeeded(session.SubscriptionId, session.Url);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe communication failure during subscription creation for Provider {ProviderId}", providerId);
            return SubscriptionGatewayResult.Failed("Payment provider communication failure.");
        }
    }

    public async Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken)
    {
        try
        {
            var subscription = await stripeService.CancelSubscriptionAsync(externalSubscriptionId, _requestOptions, cancellationToken);
            
            // subscription.CancelAtPeriodEnd é bool?, precisamos tratar a nulidade corretamente
            return subscription?.Status == "canceled" || (subscription?.CancelAtPeriodEnd ?? false);
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe error cancelling subscription {SubscriptionId}", externalSubscriptionId);
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
                ReturnUrl = returnUrl
            };

            var session = await stripeService.CreateBillingPortalSessionAsync(options, _requestOptions, cancellationToken);
            return session?.Url;
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe error creating billing portal session for Customer {CustomerId}", externalCustomerId);
            return null;
        }
    }
}
