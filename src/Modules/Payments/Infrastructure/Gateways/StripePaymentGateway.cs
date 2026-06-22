using MeAjudaAi.Modules.Payments.Application.Options;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain.ValueObjects;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Gateways;

public class StripePaymentGateway : IPaymentGateway
{
    private readonly string _successUrl;
    private readonly string _cancelUrl;
    private readonly RequestOptions _requestOptions;
    private readonly IStripeService _stripeService;
    private readonly ILogger<StripePaymentGateway> _logger;
    private readonly PaymentsOptions _options;

    public StripePaymentGateway(
        IConfiguration configuration,
        PaymentsOptions paymentsOptions,
        ILogger<StripePaymentGateway> logger,
        IStripeService stripeService)
    {
        _options = paymentsOptions;
        _logger = logger;
        _stripeService = stripeService;

        var apiKey = configuration["Stripe:ApiKey"]!;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("Stripe:ApiKey is missing or empty in configuration.");
        }

        var clientBaseUrl = (configuration["ClientBaseUrl"] ?? "").TrimEnd('/');
        if (string.IsNullOrWhiteSpace(clientBaseUrl))
        {
            throw new ArgumentException("ClientBaseUrl is missing or empty in configuration.");
        }

        _successUrl = BuildReturnUrl(clientBaseUrl, _options.SuccessUrl, "Payments:SuccessUrl");
        _cancelUrl = BuildReturnUrl(clientBaseUrl, _options.CancelUrl, "Payments:CancelUrl");

        _requestOptions = new RequestOptions { ApiKey = apiKey };
    }

    public async Task<SubscriptionGatewayResponse> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, string? idempotencyKey = null, CancellationToken cancellationToken = default)
    {
        if (CurrencyUtils.IsZeroDecimalCurrency(amount.Currency) && amount.Amount % 1 != 0)
        {
            _logger.LogWarning("Attempt to create subscription with fractional amount for zero-decimal currency: {Currency} {Amount}", amount.Currency, amount.Amount);
            return SubscriptionGatewayResponse.Failed($"Moeda zero-decimal ({amount.Currency}) não aceita valores fracionários: {amount.Amount}");
        }

        try
        {
            // O planId do domínio é resolvido para o Stripe Price ID via mapeamento de configuração
            var stripePriceId = (_options.Plans.TryGetValue(planId, out var plan) ? plan.StripePriceId : null) ?? planId;

            var price = await _stripeService.GetPriceAsync(stripePriceId, _requestOptions, cancellationToken);
            
            var expectedAmount = CurrencyUtils.ConvertToMinorUnits(amount.Amount, amount.Currency);
            if (price.UnitAmount != expectedAmount || !string.Equals(price.Currency, amount.Currency, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("Price mismatch detected. Stripe: {StripeAmount} {StripeCurrency}, Expected: {ExpectedAmount} {ExpectedCurrency}", 
                    price.UnitAmount, price.Currency, expectedAmount, amount.Currency);
                return SubscriptionGatewayResponse.Failed("O valor ou moeda do plano não corresponde às informações do provedor de pagamento.");
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = [StripeConstants.PaymentMethodCard],
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Price = stripePriceId,
                        Quantity = 1,
                    },
                ],
                Mode = "subscription",
                SuccessUrl = AppendSessionId(_successUrl),
                CancelUrl = _cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { StripeConstants.ProviderIdMetadataKey, providerId.ToString() }
                }
            };

            var session = await _stripeService.CreateCheckoutSessionAsync(options, CreateRequestOptions(idempotencyKey), cancellationToken);

            if (string.IsNullOrWhiteSpace(session.Url))
            {
                _logger.LogError("Stripe returned session with null or empty URL for Provider {ProviderId}", providerId);
                return SubscriptionGatewayResponse.Failed("Provedor de pagamento retornou URL de checkout inválida.");
            }

            return SubscriptionGatewayResponse.Succeeded(null, session.Url);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating subscription for Provider {ProviderId}", providerId);
            return SubscriptionGatewayResponse.Failed("Falha na comunicação com o provedor de pagamento.");
        }
    }

    public async Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken)
    {
        try
        {
            var subscription = await _stripeService.CancelSubscriptionAsync(externalSubscriptionId, _requestOptions, cancellationToken);
            return subscription.Status == StripeConstants.SubscriptionStatusCanceled;
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
                ReturnUrl = returnUrl
            };

            var session = await _stripeService.CreateBillingPortalSessionAsync(options, _requestOptions, cancellationToken);
            return session.Url;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error creating customer portal session for Customer {CustomerId}", externalCustomerId);
            return null;
        }
    }

    private static string BuildReturnUrl(string clientBaseUrl, string? pathConfig, string configKey)
    {
        var path = (pathConfig ?? "").Trim();
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException($"{configKey} is missing or empty in configuration.");
        }

        if (Uri.TryCreate(path, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return path;
        }

        return $"{clientBaseUrl}/{path.TrimStart('/')}";
    }

    private RequestOptions CreateRequestOptions(string? idempotencyKey = null)
    {
        var options = new RequestOptions { ApiKey = _requestOptions.ApiKey };
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            options.IdempotencyKey = idempotencyKey;
        }
        return options;
    }

    private static string AppendSessionId(string url)
    {
        if (url.Contains("{CHECKOUT_SESSION_ID}", StringComparison.Ordinal))
        {
            return url;
        }

        return url.Contains('?')
            ? $"{url}&session_id={{CHECKOUT_SESSION_ID}}"
            : $"{url}?session_id={{CHECKOUT_SESSION_ID}}";
    }
}