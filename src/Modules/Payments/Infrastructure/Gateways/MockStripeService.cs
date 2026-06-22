using MeAjudaAi.Modules.Payments.Infrastructure.Gateways;
using Stripe;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Payments.Infrastructure;

[ExcludeFromCodeCoverage]
internal class MockStripeService : IStripeService
{
    public Task<Price> GetPriceAsync(string priceId, RequestOptions? requestOptions, CancellationToken cancellationToken)
        => Task.FromResult(new Price { Id = priceId, UnitAmount = 1000, Currency = "brl" });

    public Task<Stripe.Checkout.Session> CreateCheckoutSessionAsync(Stripe.Checkout.SessionCreateOptions options, RequestOptions? requestOptions, CancellationToken cancellationToken)
        => Task.FromResult(new Stripe.Checkout.Session { Id = "mock_session", Url = "https://checkout.stripe.com/mock" });

    public Task<Stripe.BillingPortal.Session> CreateBillingPortalSessionAsync(Stripe.BillingPortal.SessionCreateOptions options, RequestOptions? requestOptions, CancellationToken cancellationToken)
        => Task.FromResult(new Stripe.BillingPortal.Session { Id = "mock_portal_session", Url = "https://billing.stripe.com/mock" });

    public Task<Subscription> CancelSubscriptionAsync(string subscriptionId, RequestOptions? requestOptions, CancellationToken cancellationToken)
        => Task.FromResult(new Subscription { Id = subscriptionId, Status = "canceled" });
}
