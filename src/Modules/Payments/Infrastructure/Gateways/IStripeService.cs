using Stripe;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Gateways;

public interface IStripeService
{
    Task<Price> GetPriceAsync(string priceId, RequestOptions? requestOptions, CancellationToken cancellationToken);
    Task<Stripe.Checkout.Session> CreateCheckoutSessionAsync(Stripe.Checkout.SessionCreateOptions options, RequestOptions? requestOptions, CancellationToken cancellationToken);
    Task<Stripe.BillingPortal.Session> CreateBillingPortalSessionAsync(Stripe.BillingPortal.SessionCreateOptions options, RequestOptions? requestOptions, CancellationToken cancellationToken);
    Task<Stripe.Subscription> CancelSubscriptionAsync(string subscriptionId, RequestOptions? requestOptions, CancellationToken cancellationToken);
}
