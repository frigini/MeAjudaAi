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
        services.AddSingleton<MeAjudaAi.Shared.Caching.ICacheService, TestCacheService>();

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

/// <summary>
/// Implementação simplificada de ICacheService para testes
/// </summary>
internal class TestCacheService : MeAjudaAi.Shared.Caching.ICacheService
{
    private readonly Dictionary<string, object> _cache = new();

    public Task<(T? value, bool isCached)> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var value) && value is T typedValue)
        {
            return Task.FromResult<(T?, bool)>((typedValue, true));
        }

        return Task.FromResult<(T?, bool)>((default, false));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions? options = null, IReadOnlyCollection<string>? tags = null, CancellationToken cancellationToken = default)
    {
        _cache[key] = value!;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var keysToRemove = _cache.Keys.Where(k => k.Contains(pattern)).ToList();
        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
        }
        return Task.CompletedTask;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, ValueTask<T>> factory, TimeSpan? expiration = null, Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions? options = null, IReadOnlyCollection<string>? tags = null, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var existingValue))
        {
            return (T)existingValue;
        }

        var newValue = await factory(cancellationToken).AsTask();
        _cache[key] = newValue!;
        return newValue;
    }

    public Task RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        // Para testes, removemos todas as chaves (implementação simplificada)
        _cache.Clear();
        return Task.CompletedTask;
    }
}
