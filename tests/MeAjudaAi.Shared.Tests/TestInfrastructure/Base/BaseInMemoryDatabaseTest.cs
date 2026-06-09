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

    protected BaseInMemoryDatabaseTest(Func<DbContextOptions<TDbContext>, TDbContext> contextFactory)
    {
        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        DbContext = contextFactory(options);
    }

    public void Dispose()
    {
        DbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
