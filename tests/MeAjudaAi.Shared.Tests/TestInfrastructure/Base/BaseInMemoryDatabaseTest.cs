using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Base;

/// <summary>
/// Classe base para testes unitários que utilizam um DbContext em memória.
/// Garante o isolamento entre testes e a limpeza correta dos recursos.
/// </summary>
public abstract class BaseInMemoryDatabaseTest<TDbContext> : IDisposable where TDbContext : DbContext
{
    protected readonly TDbContext DbContext;
    private bool _disposed;

    protected BaseInMemoryDatabaseTest(Func<DbContextOptions<TDbContext>, TDbContext> contextFactory)
    {
        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        DbContext = contextFactory(options);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // liberar recursos gerenciados
            DbContext?.Dispose();
        }

        // sem recursos não gerenciados para liberar

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
