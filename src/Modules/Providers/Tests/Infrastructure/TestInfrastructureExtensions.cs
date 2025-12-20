using MeAjudaAi.Modules.Providers.Application.Services.Interfaces;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Modules.Providers.Infrastructure.Queries;
using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using MeAjudaAi.Shared.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Modules.Providers.Tests.Infrastructure;

/// <summary>
/// Extensões para configurar infraestrutura de testes específica do módulo Providers
/// </summary>
public static class ProvidersTestInfrastructureExtensions
{
    /// <summary>
    /// Adiciona toda a infraestrutura de testes necessária para o módulo Providers
    /// </summary>
    public static IServiceCollection AddProvidersTestInfrastructure(
        this IServiceCollection services,
        TestInfrastructureOptions? options = null)
    {
        options ??= new TestInfrastructureOptions();

        services.AddSingleton(options);

        // Adicionar serviços compartilhados essenciais (incluindo IDateTimeProvider)
        services.AddSingleton<IDateTimeProvider, TestDateTimeProvider>();

        // Usar extensões compartilhadas
        services.AddTestLogging();
        services.AddTestCache(options.Cache);

        // Adicionar serviços de cache do Shared (incluindo ICacheService)
        // Para testes, usar implementação simples sem dependências complexas
        services.AddSingleton<MeAjudaAi.Shared.Caching.ICacheService, MeAjudaAi.Shared.Tests.TestInfrastructure.Services.TestCacheService>();

        // Configurar DbContext específico para PostgreSQL com TestContainers (isolado por teste)
        services.AddDbContext<ProvidersDbContext>((serviceProvider, dbOptions) =>
        {
            var container = serviceProvider.GetRequiredService<PostgreSqlContainer>();
            var connectionString = container.GetConnectionString();

            dbOptions.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Providers.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", options.Database.Schema);
                npgsqlOptions.CommandTimeout(60);
            })
            .ConfigureWarnings(warnings =>
            {
                // Suprimir warnings de pending model changes em testes
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
            });
        }, ServiceLifetime.Scoped); // Garantir que seja Scoped

        // Adicionar repositórios específicos do Providers
        services.AddScoped<IProviderRepository, ProviderRepository>();

        // Adicionar serviços de aplicação específicos do Providers
        services.AddScoped<IProviderQueryService, ProviderQueryService>();

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
