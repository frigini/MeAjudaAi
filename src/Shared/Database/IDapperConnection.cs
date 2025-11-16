namespace MeAjudaAi.Shared.Database;

public interface IDapperConnection
{
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken cancellationToken = default);
    Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, CancellationToken cancellationToken = default);
    Task<int> ExecuteAsync(string sql, object? param = null, CancellationToken cancellationToken = default);
}
