using MeAjudaAi.Modules.SearchProviders.Domain.Repositories;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure;

/// <summary>
/// Métodos de extensão para registrar serviços da camada de Infrastructure do Search.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registra serviços da camada de Infrastructure do Search.
    /// </summary>
    public static IServiceCollection AddSearchInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Registrar DbContext com suporte PostGIS
        // IMPORTANTE: EF Core usa GetConnectionString("DefaultConnection") enquanto Dapper (via PostgresOptions)
        // resolve de "Postgres:ConnectionString" ou "ConnectionStrings:meajudaai-db".
        // Certifique-se de que estas chaves de configuração apontem para o mesmo database para evitar que EF e Dapper
        // se comuniquem com databases/schemas diferentes entre ambientes (dev/test/prod, Aspire, etc.).
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                              ?? configuration.GetConnectionString("Search")
                              ?? configuration.GetConnectionString("meajudaai-db")
                              ?? throw new InvalidOperationException(
                                  "Database connection string not found. Tried: 'DefaultConnection', 'Search', 'meajudaai-db'. " +
                                  "Please configure one of these connection strings in appsettings.json or environment variables.");

        services.AddDbContext<SearchProvidersDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "search");
                npgsqlOptions.UseNetTopologySuite(); // Habilitar suporte PostGIS/geoespacial
            });

            options.UseSnakeCaseNamingConvention();

            // Habilitar erros detalhados em desenvolvimento
            if (configuration.GetValue<bool>("DetailedErrors"))
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        // Registrar Dapper para queries espaciais otimizadas
        services.AddDapper();

        // Registrar repositórios
        services.AddScoped<ISearchableProviderRepository, SearchableProviderRepository>();

        return services;
    }
}
