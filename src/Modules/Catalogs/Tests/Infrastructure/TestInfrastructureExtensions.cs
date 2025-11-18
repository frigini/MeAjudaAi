using MeAjudaAi.Modules.Catalogs.Application;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.Catalogs.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.Infrastructure;
using MeAjudaAi.Shared.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Modules.Catalogs.Tests.Infrastructure;

public static class TestInfrastructureExtensions
{
    /// <summary>
    /// Adiciona infraestrutura de teste específica do módulo Catalogs
    /// </summary>
    public static IServiceCollection AddCatalogsTestInfrastructure(
        this IServiceCollection services,
        TestInfrastructureOptions? options = null)
    {
        options ??= new TestInfrastructureOptions();

        services.AddSingleton(options);

        // Adicionar serviços compartilhados essenciais
        services.AddSingleton<IDateTimeProvider, TestDateTimeProvider>();

        // Usar extensões compartilhadas
        services.AddTestLogging();
        services.AddTestCache(options.Cache);

        // Adicionar serviços de cache do Shared
        services.AddSingleton<MeAjudaAi.Shared.Caching.ICacheService, TestCacheService>();

        // Configurar banco de dados específico do módulo Catalogs
        services.AddTestDatabase<CatalogsDbContext>(
            options.Database,
            "MeAjudaAi.Modules.Catalogs.Infrastructure");

        // Configurar DbContext específico com snake_case naming
        services.AddDbContext<CatalogsDbContext>((serviceProvider, dbOptions) =>
        {
            var container = serviceProvider.GetRequiredService<PostgreSqlContainer>();
            var connectionString = container.GetConnectionString();

            dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Catalogs.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", options.Database.Schema);
                npgsqlOptions.CommandTimeout(60);
            })
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(warnings =>
            {
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
            });
        });

        // Configurar mocks específicos do módulo Catalogs
        if (options.ExternalServices.UseMessageBusMock)
        {
            services.AddTestMessageBus();
        }

        // Adicionar repositórios específicos do Catalogs
        services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();

        // Adicionar serviços de aplicação (incluindo ICatalogsModuleApi)
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
