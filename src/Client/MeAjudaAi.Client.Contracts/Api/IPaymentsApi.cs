using MeAjudaAi.Contracts.Modules.Payments.DTOs;
using Refit;

namespace MeAjudaAi.Client.Contracts.Api;

public interface IPaymentsApi
{
    [Post("/api/payments/subscriptions")]
    Task<CreateSubscriptionResponse> CreateSubscriptionAsync(
        [Body] CreateSubscriptionRequest request,
        [Header("Idempotency-Key")] string? idempotencyKey = null,
        CancellationToken cancellationToken = default);

    [Post("/api/payments/subscriptions/billing-portal")]
    Task<GetBillingPortalResponse> GetBillingPortalAsync(
        [Body] GetBillingPortalRequest request,
        CancellationToken cancellationToken = default);
}
