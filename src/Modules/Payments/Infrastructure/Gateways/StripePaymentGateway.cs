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
        _successUrl = configuration["Payments:SuccessUrl"] ?? "";
        _cancelUrl = configuration["Payments:CancelUrl"] ?? "";

        _requestOptions = new RequestOptions
        {
            ApiKey = apiKey
        };
    }

    public async Task<SubscriptionGatewayResult> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, CancellationToken cancellationToken)
    {
        if (IsZeroDecimalCurrency(amount.Currency) && amount.Amount % 1 != 0)
        {
            _logger.LogWarning("Tentativa de criar assinatura com valor fracionado para moeda de zero decimais: {Currency} {Amount}", amount.Currency, amount.Amount);
            return new SubscriptionGatewayResult(false, null, null, $"Moedas sem casas decimais ({amount.Currency}) não aceitam valores fracionados: {amount.Amount}");
        }

        try
        {
            var price = await _stripeService.GetPriceAsync(planId, _requestOptions, cancellationToken);
            
            var expectedAmount = ConvertToMinorUnits(amount.Currency, amount.Amount);
            if (price.UnitAmount != expectedAmount || price.Currency != amount.Currency.ToLowerInvariant())
            {
                _logger.LogError("Divergência de preço detectada. Stripe: {StripeAmount} {StripeCurrency}, Esperado: {ExpectedAmount} {ExpectedCurrency}", 
                    price.UnitAmount, price.Currency, expectedAmount, amount.Currency);
                return new SubscriptionGatewayResult(false, null, null, "O valor ou moeda do plano não coincide com as informações do provedor de pagamento.");
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
            _logger.LogError(ex, "Erro no Stripe ao criar assinatura para o Provider {ProviderId}", providerId);
            return new SubscriptionGatewayResult(false, null, null, "Falha na comunicação com o provedor de pagamento.");
        }
    }

    public Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken)
    {
        // Implementação real usaria SubscriptionService.CancelAsync via IStripeService
        _logger.LogInformation("Cancelamento de assinatura solicitado para {ExternalId} (Simulado)", externalSubscriptionId);
        return Task.FromResult(true);
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
            _logger.LogError(ex, "Erro no Stripe ao criar sessão do portal para o Cliente {CustomerId}", externalCustomerId);
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
