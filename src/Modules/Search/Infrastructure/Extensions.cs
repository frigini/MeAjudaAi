using MeAjudaAi.Modules.Search.Domain.Repositories;
using MeAjudaAi.Modules.Search.Infrastructure.Persistence;
using MeAjudaAi.Modules.Search.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Search.Infrastructure;

/// <summary>
/// Extension methods for registering Search Infrastructure layer services.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers Search Infrastructure layer services.
    /// </summary>
    public static IServiceCollection AddSearchInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext with PostGIS support
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string 'DefaultConnection' not found.");

        services.AddDbContext<SearchDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "search");
                npgsqlOptions.UseNetTopologySuite(); // Enable PostGIS/geospatial support
            });

            options.UseSnakeCaseNamingConvention();

            // Enable detailed errors in development
            if (configuration.GetValue<bool>("DetailedErrors"))
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        // Register repositories
        services.AddScoped<ISearchableProviderRepository, SearchableProviderRepository>();

        return services;
    }
}
