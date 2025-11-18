using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MeAjudaAi.Modules.Catalogs.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for creating CatalogsDbContext during EF Core migrations.
/// This allows migrations to be created without running the full application.
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
