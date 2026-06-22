using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Modules.Payments.DTOs;
using Refit;

namespace MeAjudaAi.Client.Contracts.Api;

public interface IPaymentsApi
{
    [Post($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Payments.Base}{ApiEndpoints.Payments.CreateSubscription}")]
    Task<CreateSubscriptionResponse> CreateSubscriptionAsync(
        [Body] CreateSubscriptionRequest request,
        [Header("Idempotency-Key")] string? idempotencyKey = null,
        CancellationToken cancellationToken = default);

    [Post($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.Payments.Base}{ApiEndpoints.Payments.GetBillingPortal}")]
    Task<GetBillingPortalResponse> GetBillingPortalAsync(
        [Body] GetBillingPortalRequest request,
        CancellationToken cancellationToken = default);
}