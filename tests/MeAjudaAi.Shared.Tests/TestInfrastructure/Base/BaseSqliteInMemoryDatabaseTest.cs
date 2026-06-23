using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Base;

/// <summary>
/// Classe base para testes unitários que utilizam um DbContext com SQLite in-memory.
/// Gerencia automaticamente a vida útil da conexão e do contexto, resolvendo CA1063.
/// </summary>
/// <typeparam name="TDbContext">Tipo do DbContext a ser testado.</typeparam>
public abstract class BaseSqliteInMemoryDatabaseTest<TDbContext> : IDisposable
    where TDbContext : DbContext
{
    protected readonly SqliteConnection Connection;
    protected readonly TDbContext DbContext;
    private bool _disposed;

    protected BaseSqliteInMemoryDatabaseTest(Func<DbContextOptions<TDbContext>, TDbContext> contextFactory)
    {
        Connection = new SqliteConnection("DataSource=:memory:");
        Connection.Open();

        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseSqlite(Connection)
            .Options;

        DbContext = contextFactory(options);
        DbContext.Database.EnsureCreated();
    }

    /// <summary>
    /// Cria um <see cref="ServiceProvider"/> com os serviços configurados.
    /// Útil para testes que precisam de DI (UnitOfWork, repositorios, etc.).
    /// </summary>
    protected ServiceProvider BuildServiceProvider(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        return services.BuildServiceProvider();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            DbContext?.Dispose();
            Connection?.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
