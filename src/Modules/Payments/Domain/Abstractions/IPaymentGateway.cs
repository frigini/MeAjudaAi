using MeAjudaAi.Shared.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Payments.Domain.Abstractions;

public interface IPaymentGateway
{
    Task<SubscriptionGatewayResult> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, CancellationToken cancellationToken);
    Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken);
    Task<string?> CreateBillingPortalSessionAsync(string externalCustomerId, string returnUrl, CancellationToken cancellationToken);
}

public record SubscriptionGatewayResult
{
    public bool Success { get; }
    public string? ExternalSubscriptionId { get; }
    public string? CheckoutUrl { get; }
    public string? ErrorMessage { get; }

    private SubscriptionGatewayResult(bool success, string? externalSubscriptionId, string? checkoutUrl, string? errorMessage)
    {
        Success = success;
        ExternalSubscriptionId = externalSubscriptionId;
        CheckoutUrl = checkoutUrl;
        ErrorMessage = errorMessage;
    }

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
