using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain.ValueObjects;
using System.Collections.Concurrent;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Mocks.Modules.Payments;

public class MockPaymentGateway : IPaymentGateway
{
    private readonly Dictionary<string, SubscriptionGatewayResponse> _subscriptionResponses = new();
    private readonly Dictionary<string, bool> _cancelResults = new();
    private readonly Dictionary<string, string?> _portalSessionUrls = new();

    public ConcurrentQueue<object> RecordedCalls { get; } = new();
    public bool ShouldFail { get; set; }

    public void SetupCreateSubscriptionResponse(Guid providerId, SubscriptionGatewayResponse response)
        => _subscriptionResponses[providerId.ToString()] = response;

    public void SetupCancelSubscriptionResponse(string externalId, bool result)
        => _cancelResults[externalId] = result;

    public void SetupBillingPortalSession(string customerId, string? url)
        => _portalSessionUrls[customerId] = url;

    public Task<SubscriptionGatewayResponse> CreateSubscriptionAsync(
        Guid providerId, string planId, Money amount, string? idempotencyKey = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RecordedCalls.Enqueue(new CreateSubscriptionCall(providerId, planId, amount, idempotencyKey));

        if (ShouldFail)
            return Task.FromResult(SubscriptionGatewayResponse.Failed("Mocked failure"));

        if (_subscriptionResponses.TryGetValue(providerId.ToString(), out var response))
            return Task.FromResult(response);

        var token = Guid.NewGuid().ToString("n");
        return Task.FromResult(SubscriptionGatewayResponse.Succeeded(
            "sub_mock_" + token,
            "https://checkout.stripe.com/mock_" + token));
    }

    public Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RecordedCalls.Enqueue(new CancelSubscriptionCall(externalSubscriptionId));

        if (ShouldFail)
            return Task.FromResult(false);

        if (_cancelResults.TryGetValue(externalSubscriptionId, out var result))
            return Task.FromResult(result);

        return Task.FromResult(true);
    }

    public Task<string?> CreateBillingPortalSessionAsync(string externalCustomerId, string returnUrl, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RecordedCalls.Enqueue(new CreateBillingPortalCall(externalCustomerId, returnUrl));

        if (ShouldFail)
            return Task.FromResult<string?>(null);

        if (_portalSessionUrls.TryGetValue(externalCustomerId, out var url))
            return Task.FromResult(url);

        return Task.FromResult($"https://billing.stripe.com/mock_portal_{Guid.NewGuid():N}");
    }

    public record CreateSubscriptionCall(Guid ProviderId, string PlanId, Money Amount, string? IdempotencyKey);
    public record CancelSubscriptionCall(string ExternalSubscriptionId);
    public record CreateBillingPortalCall(string ExternalCustomerId, string ReturnUrl);
}
