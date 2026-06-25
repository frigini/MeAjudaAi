using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;

/// <summary>
/// Factory para criação do SearchProvidersDbContext em design time (para migrações).
/// </summary>
public class SearchProvidersDbContextFactory : IDesignTimeDbContextFactory<SearchProvidersDbContext>
{
    public SearchProvidersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SearchProvidersDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? throw new InvalidOperationException("DB_CONNECTION_STRING environment variable is required.");

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(SearchProvidersDbContext).Assembly);
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.SearchProviders);
            npgsqlOptions.UseNetTopologySuite();
        });

        optionsBuilder.UseSnakeCaseNamingConvention();

        return new SearchProvidersDbContext(optionsBuilder.Options);
    }
}
