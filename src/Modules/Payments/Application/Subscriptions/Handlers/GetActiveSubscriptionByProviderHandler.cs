using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Payments.Application.Queries;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Payments.Application.Subscriptions.Handlers;

public class GetActiveSubscriptionByProviderHandler(ISubscriptionQueries subscriptionQueries) 
    : IQueryHandler<GetActiveSubscriptionByProviderQuery, Result<Subscription?>>
{
    public async Task<Result<Subscription?>> HandleAsync(GetActiveSubscriptionByProviderQuery query, CancellationToken cancellationToken = default)
    {
        var subscription = await subscriptionQueries.GetActiveByProviderIdAsync(query.ProviderId, cancellationToken);
        return Result<Subscription?>.Success(subscription);
    }
}
