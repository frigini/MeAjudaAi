using MeAjudaAi.Shared.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Payments.Domain.Abstractions;

public interface IPaymentGateway
{
    Task<SubscriptionGatewayResult> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, CancellationToken cancellationToken);
    Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken);
    Task<string?> CreateBillingPortalSessionAsync(string externalCustomerId, string returnUrl, CancellationToken cancellationToken);
}

public record SubscriptionGatewayResult(
    bool Success,
    string? ExternalSubscriptionId,
    string? CheckoutUrl,
    string? ErrorMessage);
