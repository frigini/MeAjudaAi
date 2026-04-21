using System.Collections.Concurrent;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Shared.Domain.ValueObjects;

namespace MeAjudaAi.Integration.Tests.Mocks;

public class MockPaymentGateway : IPaymentGateway
{
    public ConcurrentQueue<object> RecordedCalls { get; } = new();
    public bool ShouldFail { get; set; }

    public Task<SubscriptionGatewayResult> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, string? idempotencyKey = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RecordedCalls.Enqueue(new CreateSubscriptionCall(providerId, planId, amount, idempotencyKey));

        if (ShouldFail) return Task.FromResult(SubscriptionGatewayResult.Failed("Mocked failure"));

        var token = Guid.NewGuid().ToString("n");
        return Task.FromResult(SubscriptionGatewayResult.Succeeded(
            "sub_mock_" + token, 
            "https://checkout.stripe.com/mock_" + token));
    }

    public Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RecordedCalls.Enqueue(new CancelSubscriptionCall(externalSubscriptionId));
        return Task.FromResult(!ShouldFail);
    }

    public Task<string?> CreateBillingPortalSessionAsync(string externalCustomerId, string returnUrl, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RecordedCalls.Enqueue(new CreateBillingPortalCall(externalCustomerId, returnUrl));
        return Task.FromResult<string?>(ShouldFail ? null : "https://billing.stripe.com/mock_portal_" + Guid.NewGuid().ToString("n"));
    }

    public record CreateSubscriptionCall(Guid ProviderId, string PlanId, Money Amount, string? IdempotencyKey);
    public record CancelSubscriptionCall(string ExternalSubscriptionId);
    public record CreateBillingPortalCall(string ExternalCustomerId, string ReturnUrl);
}
