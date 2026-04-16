using Stripe;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Gateways;

public interface IStripeService
{
    Task<Price> GetPriceAsync(string priceId, RequestOptions? requestOptions, CancellationToken cancellationToken);
    Task<Stripe.Checkout.Session> CreateCheckoutSessionAsync(Stripe.Checkout.SessionCreateOptions options, RequestOptions? requestOptions, CancellationToken cancellationToken);
    Task<string> CreateBillingPortalSessionAsync(Stripe.BillingPortal.SessionCreateOptions options, RequestOptions? requestOptions, CancellationToken cancellationToken);
}

public class StripeService : IStripeService
{
    public Task<Price> GetPriceAsync(string priceId, RequestOptions? requestOptions, CancellationToken cancellationToken)
    {
        return new PriceService().GetAsync(priceId, null, requestOptions, cancellationToken);
    }

    public Task<Stripe.Checkout.Session> CreateCheckoutSessionAsync(Stripe.Checkout.SessionCreateOptions options, RequestOptions? requestOptions, CancellationToken cancellationToken)
    {
        return new Stripe.Checkout.SessionService().CreateAsync(options, requestOptions, cancellationToken);
    }

    public Task<string> CreateBillingPortalSessionAsync(Stripe.BillingPortal.SessionCreateOptions options, RequestOptions? requestOptions, CancellationToken cancellationToken)
    {
        return new Stripe.BillingPortal.SessionService().CreateAsync(options, requestOptions, cancellationToken).ContinueWith(t => t.Result.Url);
    }
}
