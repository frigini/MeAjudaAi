using MeAjudaAi.Shared.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Payments.Domain.Abstractions;

public interface IPaymentGateway
{
    Task<SubscriptionGatewayResult> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, CancellationToken cancellationToken, string? idempotencyKey = null);
    Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken);
    Task<string?> CreateBillingPortalSessionAsync(string externalCustomerId, string returnUrl, CancellationToken cancellationToken);
}

public record SubscriptionGatewayResult(
    bool Success,
    string? ExternalSubscriptionId,
    string? CheckoutUrl,
    string? ErrorMessage)
{
    public static SubscriptionGatewayResult Succeeded(string? externalSubscriptionId, string checkoutUrl)
    {
        if (string.IsNullOrWhiteSpace(checkoutUrl))
            throw new ArgumentException("CheckoutUrl is required for successful result.", nameof(checkoutUrl));

        return new SubscriptionGatewayResult(true, externalSubscriptionId, checkoutUrl, null);
    }

    public static SubscriptionGatewayResult SucceededWithoutExternalId(string checkoutUrl)
    {
        if (string.IsNullOrWhiteSpace(checkoutUrl))
            throw new ArgumentException("CheckoutUrl is required for successful result.", nameof(checkoutUrl));

        return new SubscriptionGatewayResult(true, null, checkoutUrl, null);
    }

    public static SubscriptionGatewayResult Failed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("ErrorMessage is required for failed result.", nameof(errorMessage));

        return new SubscriptionGatewayResult(false, null, null, errorMessage);
    }
}
