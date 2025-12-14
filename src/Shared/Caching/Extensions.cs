using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Shared.Caching;

internal static class Extensions
{
    public static IServiceCollection AddCaching(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = 1024 * 1024;
            options.MaximumKeyLength = 1024;
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(1),
                LocalCacheExpiration = TimeSpan.FromMinutes(5)
            };
        });

        // Redis como cache distribuído (HybridCache usa automaticamente)
        services.AddStackExchangeRedisCache(options =>
        {
            // Tenta múltiplas fontes de string de conexão Redis em ordem de preferência
            options.Configuration =
                configuration.GetConnectionString("redis") ??          // Nome padrão Aspire
                configuration.GetConnectionString("Redis") ??          // Configuração manual
                "localhost:6379";                                      // Fallback para testes
            options.InstanceName = "MeAjudaAi";
        });

        // Registra métricas de cache
        services.AddSingleton<CacheMetrics>();

        // Registra serviços de cache
        services.AddSingleton<ICacheService, HybridCacheService>();

        return services;
    }
}
