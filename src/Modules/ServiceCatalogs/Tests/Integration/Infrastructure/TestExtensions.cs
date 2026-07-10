using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Modules.ServiceCatalogs.Application;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Queries;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Caching.Interfaces;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Integration.Infrastructure;

public static class TestExtensions
{
    /// <summary>
    /// Adiciona infraestrutura de teste específica do módulo ServiceCatalogs
    /// </summary>
    public static IServiceCollection AddServiceCatalogsTestInfrastructure(
        this IServiceCollection services,
        TestInfrastructureOptions? options = null)
    {
        options ??= new TestInfrastructureOptions();

        options.Database ??= new TestDatabaseOptions();
        options.Cache ??= new TestCacheOptions();
        options.ExternalServices ??= new TestExternalServicesOptions();

        if (string.IsNullOrEmpty(options.Database.Schema))
        {
            options.Database.Schema = "public";
        }

        services.AddSingleton(options);

        services.AddSingleton(TimeProvider.System);

        services.AddTestLogging();
        services.AddTestCache(options.Cache);

        services.AddLocalization();

        services.AddSingleton<ICacheService, MeAjudaAi.Shared.Tests.TestInfrastructure.Services.TestCacheService>();

        services.AddDbContext<ServiceCatalogsDbContext>((serviceProvider, dbOptions) =>
        {
            var container = serviceProvider.GetRequiredService<PostgreSqlContainer>();
            var connectionString = container.GetConnectionString();

            dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ServiceCatalogsDbContext).Assembly.FullName);
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", options.Database.Schema);
                npgsqlOptions.CommandTimeout(60);
            })
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(warnings =>
            {
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
            });
        });

        if (options.ExternalServices.UseMessageBusMock)
        {
            services.AddTestMessageBus();
        }

        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.ServiceCatalogs, (sp, key) => sp.GetRequiredService<ServiceCatalogsDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ServiceCatalogsDbContext>());
        services.AddScoped<IServiceCategoryQueries, DbContextServiceCategoryQueries>();
        services.AddScoped<IServiceQueries, DbContextServiceQueries>();

        services.AddApplication();

        return services;
    }
}
