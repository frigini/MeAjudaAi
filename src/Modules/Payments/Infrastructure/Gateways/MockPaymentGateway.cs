using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain.ValueObjects;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Payments.Infrastructure;

[ExcludeFromCodeCoverage]
internal class MockPaymentGateway : IPaymentGateway
{
    public Task<SubscriptionGatewayResponse> CreateSubscriptionAsync(Guid providerId, string planId, Money amount, string? idempotencyKey = null, CancellationToken cancellationToken = default)
        => Task.FromResult(SubscriptionGatewayResponse.Succeeded(null, "https://checkout.stripe.com/mock"));

    public Task<bool> CancelSubscriptionAsync(string externalSubscriptionId, CancellationToken cancellationToken)
        => Task.FromResult(true);

    public Task<string?> CreateBillingPortalSessionAsync(string externalCustomerId, string returnUrl, CancellationToken cancellationToken)
        => Task.FromResult<string?>("https://billing.stripe.com/mock");
}
