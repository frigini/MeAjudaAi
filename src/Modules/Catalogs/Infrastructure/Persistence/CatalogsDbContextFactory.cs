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

        // Usa string de conexão da variável de ambiente para operações em tempo de design
        // Isso é usado apenas para geração de migrações, não em runtime
        var connectionString = Environment.GetEnvironmentVariable("CATALOGS_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=meajudaai;Username=postgres;Password=development123";

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "catalogs");
                npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

        return new CatalogsDbContext(optionsBuilder.Options);
    }
}
