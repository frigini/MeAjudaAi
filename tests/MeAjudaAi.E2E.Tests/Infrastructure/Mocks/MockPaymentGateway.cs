using System.Collections.Concurrent;
using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain.ValueObjects;

namespace MeAjudaAi.E2E.Tests.Infrastructure.Mocks;

public class MockPaymentGateway : IPaymentGateway
{
    public ConcurrentQueue<object> RecordedCalls { get; } = new();
    public bool ShouldFail { get; set; }

    public Task<SubscriptionGatewayResponse> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, string? idempotencyKey = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RecordedCalls.Enqueue(new { providerId, planId, amount, idempotencyKey });
        
        if (ShouldFail) return Task.FromResult(SubscriptionGatewayResponse.Failed("Mocked failure"));

        var token = Guid.NewGuid().ToString("n");
        return Task.FromResult(SubscriptionGatewayResponse.Succeeded(
            "sub_mock_" + token, 
            "https://checkout.stripe.com/mock_" + token));
    }

    public Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken)
    {
        RecordedCalls.Enqueue(new { externalSubscriptionId });
        return Task.FromResult(!ShouldFail);
    }

    public Task<string?> CreateBillingPortalSessionAsync(string externalCustomerId, string returnUrl, CancellationToken cancellationToken)
    {
        RecordedCalls.Enqueue(new { externalCustomerId, returnUrl });
        return Task.FromResult<string?>(ShouldFail ? null : "https://billing.stripe.com/mock_session");
    }

    public record CreateSubscriptionCall(Guid ProviderId, string PlanId, Money Amount, string? IdempotencyKey);
    public record CancelSubscriptionCall(string ExternalSubscriptionId);
    public record CreateBillingPortalCall(string ExternalCustomerId, string ReturnUrl);
}
