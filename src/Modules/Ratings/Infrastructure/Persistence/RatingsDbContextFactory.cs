using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;

/// <summary>
/// Factory para criação do RatingsDbContext em design time (para migrações)
/// O nome do módulo é detectado automaticamente do namespace
/// </summary>
public class RatingsDbContextFactory : BaseDesignTimeDbContextFactory<RatingsDbContext>
{
    protected override RatingsDbContext CreateDbContextInstance(DbContextOptions<RatingsDbContext> options)
    {
        return new RatingsDbContext(options, null!);
    }
}
