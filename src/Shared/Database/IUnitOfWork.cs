using MeAjudaAi.Shared.Database.Outbox;

namespace MeAjudaAi.Shared.Database;

public interface IUnitOfWork
{
    IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>();
    IOutboxRepository<OutboxMessage> GetOutboxRepository();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}