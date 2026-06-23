using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Payments.Application.Queries;
using MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Payments.Domain.Entities;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Payments.Application.Handlers.Subscriptions.Queries;

public class GetActiveSubscriptionByProviderQueryHandler(ISubscriptionQueries subscriptionQueries) 
    : IQueryHandler<GetActiveSubscriptionByProviderQuery, Result<Subscription?>>
{
    public async Task<Result<Subscription?>> HandleAsync(GetActiveSubscriptionByProviderQuery query, CancellationToken cancellationToken = default)
    {
        var subscription = await subscriptionQueries.GetActiveByProviderIdAsync(query.ProviderId, cancellationToken);
        return Result<Subscription?>.Success(subscription);
    }
}
