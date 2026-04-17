using Stripe;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Gateways;

public interface IStripeService
{
    Task<Price> GetPriceAsync(string priceId, RequestOptions? requestOptions, CancellationToken cancellationToken);
    Task<Stripe.Checkout.Session> CreateCheckoutSessionAsync(Stripe.Checkout.SessionCreateOptions options, RequestOptions? requestOptions, CancellationToken cancellationToken);
    Task<Stripe.BillingPortal.Session> CreateBillingPortalSessionAsync(Stripe.BillingPortal.SessionCreateOptions options, RequestOptions? requestOptions, CancellationToken cancellationToken);
    Task<Stripe.Subscription> CancelSubscriptionAsync(string subscriptionId, RequestOptions? requestOptions, CancellationToken cancellationToken);
}

public class StripeService(IStripeClient stripeClient) : IStripeService
{
    private readonly PriceService _priceService = new(stripeClient);
    private readonly Stripe.Checkout.SessionService _checkoutSessionService = new(stripeClient);
    private readonly Stripe.BillingPortal.SessionService _billingPortalSessionService = new(stripeClient);
    private readonly Stripe.SubscriptionService _subscriptionService = new(stripeClient);

    public Task<Price> GetPriceAsync(string priceId, RequestOptions? requestOptions, CancellationToken cancellationToken)
    {
        return _priceService.GetAsync(priceId, null, requestOptions, cancellationToken);
    }

    public Task<Stripe.Checkout.Session> CreateCheckoutSessionAsync(Stripe.Checkout.SessionCreateOptions options, RequestOptions? requestOptions, CancellationToken cancellationToken)
    {
        return _checkoutSessionService.CreateAsync(options, requestOptions, cancellationToken);
    }

    public Task<Stripe.BillingPortal.Session> CreateBillingPortalSessionAsync(Stripe.BillingPortal.SessionCreateOptions options, RequestOptions? requestOptions, CancellationToken cancellationToken)
    {
        return _billingPortalSessionService.CreateAsync(options, requestOptions, cancellationToken);
    }

    public Task<Stripe.Subscription> CancelSubscriptionAsync(string subscriptionId, RequestOptions? requestOptions, CancellationToken cancellationToken)
    {
        return _subscriptionService.CancelAsync(subscriptionId, null, requestOptions, cancellationToken);
    }
}
