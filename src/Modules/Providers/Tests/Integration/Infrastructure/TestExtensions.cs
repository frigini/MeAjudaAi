using MeAjudaAi.Modules.Providers.Application;
using MeAjudaAi.Modules.Providers.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Queries;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.FeatureManagement;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Tests.Integration.Infrastructure;

[ExcludeFromCodeCoverage]
public static class TestExtensions
{
    public static IServiceCollection AddProvidersTestInfrastructure(
        this IServiceCollection services,
        TestInfrastructureOptions? options = null)
    {
        options ??= new TestInfrastructureOptions();

        services.AddSingleton(options);

        services.AddSingleton(TimeProvider.System);
        services.AddTestLogging();
        services.AddTestCache(options.Cache);
        services.AddLocalization();

        var configBuilder = new ConfigurationBuilder();
        if (options.FeatureFlags?.Any() == true)
        {
            configBuilder.AddInMemoryCollection(
                options.FeatureFlags.Select(kv => new KeyValuePair<string, string?>($"FeatureManagement:{kv.Key}", kv.Value.ToString())));
        }
        services.AddSingleton<IConfiguration>(configBuilder.Build());

        services.AddFeatureManagement();

        services.AddSingleton<ICacheService, TestCacheService>();

        services.AddDbContext<ProvidersDbContext>((serviceProvider, dbOptions) =>
        {
            var connectionString = options.Database.ConnectionString
                ?? throw new InvalidOperationException("Connection string is required for integration tests.");

            dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Providers.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", options.Database.Schema);
                npgsqlOptions.CommandTimeout(60);
            });

            dbOptions.EnableServiceProviderCaching();
            dbOptions.EnableSensitiveDataLogging(false);

            dbOptions.ConfigureWarnings(warnings =>
            {
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
            });
        }, ServiceLifetime.Scoped);

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ProvidersDbContext>());
        services.AddScoped<IProviderQueries, DbContextProviderQueries>();

        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();

        services.AddApplication();

        return services;
    }
}
