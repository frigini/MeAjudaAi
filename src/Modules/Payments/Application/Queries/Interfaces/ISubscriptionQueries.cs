using MeAjudaAi.Modules.Payments.Domain.Entities;

namespace MeAjudaAi.Modules.Payments.Application.Queries.Interfaces;

public interface ISubscriptionQueries
{
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Subscription?> GetActiveByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetLatestByProviderIdAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetByExternalIdAsync(string externalSubscriptionId, CancellationToken cancellationToken = default);
}
