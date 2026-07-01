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

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Integration;

public static class TestInfrastructureExtensions
{
    /// <summary>
    /// Adiciona infraestrutura de teste específica do módulo ServiceCatalogs
    /// </summary>
    public static IServiceCollection AddServiceCatalogsTestInfrastructure(
        this IServiceCollection services,
        TestInfrastructureOptions? options = null)
    {
        options ??= new TestInfrastructureOptions();

        // Initialize nested options to ensure non-null properties
        options.Database ??= new TestDatabaseOptions();
        options.Cache ??= new TestCacheOptions();
        options.ExternalServices ??= new TestExternalServicesOptions();

        // Set default schema if not provided
        if (string.IsNullOrEmpty(options.Database.Schema))
        {
            options.Database.Schema = "public";
        }

        services.AddSingleton(options);

        // Adicionar serviços compartilhados essenciais
        services.AddSingleton(TimeProvider.System);

        // Usar extensões compartilhadas
        services.AddTestLogging();
        services.AddTestCache(options.Cache);

        // Adicionar suporte a localização
        services.AddLocalization();

        // Adicionar serviços de cache do Shared
        services.AddSingleton<MeAjudaAi.Shared.Caching.ICacheService, MeAjudaAi.Shared.Tests.TestInfrastructure.Services.TestCacheService>();

        // Configurar banco de dados específico do módulo ServiceCatalogs
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

        // Configurar mocks específicos do módulo ServiceCatalogs
        if (options.ExternalServices.UseMessageBusMock)
        {
            services.AddTestMessageBus();
        }

        // Configuração do Unit of Work e Queries
        services.AddKeyedScoped<IUnitOfWork>(ModuleKeys.ServiceCatalogs, (sp, key) => sp.GetRequiredService<ServiceCatalogsDbContext>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ServiceCatalogsDbContext>());
        services.AddScoped<IServiceCategoryQueries, DbContextServiceCategoryQueries>();
        services.AddScoped<IServiceQueries, DbContextServiceQueries>();

        // Adicionar serviços de aplicação (incluindo IServiceCatalogsModuleApi)
        services.AddApplication();

        return services;
    }
}



