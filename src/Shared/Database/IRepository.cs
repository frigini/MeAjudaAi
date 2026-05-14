namespace MeAjudaAi.Shared.Database;

public interface IRepository<TAggregate, TKey>
{
    Task<TAggregate?> TryFindAsync(TKey key, CancellationToken cancellationToken = default);
    void Add(TAggregate aggregate);
    void Delete(TAggregate aggregate);

    async Task DeleteAsync(TKey key, CancellationToken cancellationToken = default)
    {
        var aggregate = await TryFindAsync(key, cancellationToken)
            ?? throw new ArgumentException($"Aggregate with key '{key}' not found.");
        Delete(aggregate);
    }
}