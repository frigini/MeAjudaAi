using MeAjudaAi.Modules.ServiceCatalogs.Application;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.Infrastructure;
using MeAjudaAi.Shared.Tests.Infrastructure.Options;
using MeAjudaAi.Shared.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Infrastructure;

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
        services.AddSingleton<IDateTimeProvider, TestDateTimeProvider>();

        // Usar extensões compartilhadas
        services.AddTestLogging();
        services.AddTestCache(options.Cache);

        // Adicionar serviços de cache do Shared
        services.AddSingleton<MeAjudaAi.Shared.Caching.ICacheService, MeAjudaAi.Shared.Tests.Infrastructure.TestCacheService>();

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

        // Adicionar repositórios específicos do ServiceCatalogs
        services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();

        // Adicionar serviços de aplicação (incluindo IServiceCatalogsModuleApi)
        services.AddApplication();

        return services;
    }
}

/// <summary>
/// Implementação de IDateTimeProvider para testes
/// </summary>
internal class TestDateTimeProvider : IDateTimeProvider
{
    public DateTime CurrentDate() => DateTime.UtcNow;
}
