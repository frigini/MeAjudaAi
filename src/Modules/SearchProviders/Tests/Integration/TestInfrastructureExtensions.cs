using MeAjudaAi.Modules.SearchProviders.Application.Queries.Interfaces;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Queries;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.SearchProviders.Tests.Integration;

public static class TestInfrastructureExtensions
{
    public static IServiceCollection AddSearchProvidersTestInfrastructure(
        this IServiceCollection services,
        TestInfrastructureOptions options)
    {
        services.AddTestLogging();

        services.AddDbContext<SearchProvidersDbContext>(dbOptions =>
        {
            dbOptions.UseNpgsql(
                options.Database.ConnectionString
                    ?? throw new InvalidOperationException("Connection string is required for SearchProviders integration tests."),
                npgsqlOptions =>
                {
                    npgsqlOptions.UseNetTopologySuite();
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", options.Database.Schema);
                });

            dbOptions.UseSnakeCaseNamingConvention();
            dbOptions.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        services.AddSingleton(new PostgresOptions { ConnectionString = options.Database.ConnectionString! });
        services.AddMetrics();
        services.AddDatabaseMonitoring();
        services.AddScoped<IDapperConnection, DapperConnection>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<SearchProvidersDbContext>());
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.SearchProviders, (sp, key) => sp.GetRequiredService<SearchProvidersDbContext>());
        services.AddScoped<ISearchableProviderQueries, DbContextSearchableProviderQueries>();

        return services;
    }
}
