namespace MeAjudaAi.Shared.Database;

public interface IUnitOfWork
{
    IRepository<TAggregate, TKey> GetRepository<TAggregate, TKey>();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}