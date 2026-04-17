using MeAjudaAi.Shared.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Payments.Domain.Abstractions;

public interface IPaymentGateway
{
    Task<SubscriptionGatewayResult> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, string? idempotencyKey = null, CancellationToken cancellationToken = default);
    Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken = default);
    Task<string?> CreateBillingPortalSessionAsync(string externalCustomerId, string returnUrl, CancellationToken cancellationToken = default);
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

    public static SubscriptionGatewayResult Failed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("ErrorMessage is required for failed result.", nameof(errorMessage));

        return new SubscriptionGatewayResult(false, null, null, errorMessage);
    }
}
