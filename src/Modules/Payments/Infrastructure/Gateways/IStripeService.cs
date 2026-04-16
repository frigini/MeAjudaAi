using Stripe;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Gateways;

public interface IStripeService
{
    Task<Price> GetPriceAsync(string priceId, RequestOptions? requestOptions, CancellationToken cancellationToken);
    Task<Stripe.Checkout.Session> CreateCheckoutSessionAsync(Stripe.Checkout.SessionCreateOptions options, RequestOptions? requestOptions, CancellationToken cancellationToken);
    Task<string?> CreateBillingPortalSessionAsync(Stripe.BillingPortal.SessionCreateOptions options, RequestOptions? requestOptions, CancellationToken cancellationToken);
    Task<bool> CancelSubscriptionAsync(string subscriptionId, RequestOptions? requestOptions, CancellationToken cancellationToken);
}

public class StripeService : IStripeService
{
    private readonly PriceService _priceService = new();
    private readonly Stripe.Checkout.SessionService _checkoutSessionService = new();
    private readonly Stripe.BillingPortal.SessionService _billingPortalSessionService = new();
    private readonly Stripe.SubscriptionService _subscriptionService = new();

    public Task<Price> GetPriceAsync(string priceId, RequestOptions? requestOptions, CancellationToken cancellationToken)
    {
        return _priceService.GetAsync(priceId, null, requestOptions, cancellationToken);
    }

    public Task<Stripe.Checkout.Session> CreateCheckoutSessionAsync(Stripe.Checkout.SessionCreateOptions options, RequestOptions? requestOptions, CancellationToken cancellationToken)
    {
        return _checkoutSessionService.CreateAsync(options, requestOptions, cancellationToken);
    }

    public async Task<string?> CreateBillingPortalSessionAsync(Stripe.BillingPortal.SessionCreateOptions options, RequestOptions? requestOptions, CancellationToken cancellationToken)
    {
        var session = await _billingPortalSessionService.CreateAsync(options, requestOptions, cancellationToken);
        return session.Url;
    }

    public async Task<bool> CancelSubscriptionAsync(string subscriptionId, RequestOptions? requestOptions, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionService.CancelAsync(subscriptionId, null, requestOptions, cancellationToken);
        return subscription.Status == "canceled";
    }
}
