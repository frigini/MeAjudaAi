using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MeAjudaAi.Modules.Catalogs.Infrastructure.Persistence;

/// <summary>
/// Fábrica em tempo de design para criar CatalogsDbContext durante as migrações do EF Core.
/// Isso permite que as migrações sejam criadas sem executar a aplicação completa.
/// </summary>
public sealed class CatalogsDbContextFactory : IDesignTimeDbContextFactory<CatalogsDbContext>
{
    public CatalogsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CatalogsDbContext>();

        // Use default development connection string for design-time operations
        // This is only used for migrations generation, not runtime
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=meajudaai;Username=postgres;Password=development123",
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "catalogs");
                npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

        return new CatalogsDbContext(optionsBuilder.Options);
    }
}
