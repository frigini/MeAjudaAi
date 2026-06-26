using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence;

/// <summary>
/// Factory para criação do ProvidersDbContext durante o design-time.
/// Usado pelo Entity Framework para executar migrações.
/// </summary>
public class ProvidersDbContextFactory : BaseDesignTimeDbContextFactory<ProvidersDbContext>
{
    protected override ProvidersDbContext CreateDbContextInstance(DbContextOptions<ProvidersDbContext> options)
    {
        return new ProvidersDbContext(options);
    }
}
