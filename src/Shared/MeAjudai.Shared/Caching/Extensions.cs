using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Metrics;

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

        // Redis como distributed cache (HybridCache usa automaticamente)
        services.AddStackExchangeRedisCache(options =>
        {
            // Try multiple Redis connection string sources in order of preference
            options.Configuration = 
                configuration.GetConnectionString("redis") ??          // Aspire naming
                configuration.GetConnectionString("Redis") ??          // Manual configuration
                "localhost:6379";                                       // Fallback for testing
            options.InstanceName = "MeAjudaAi";
        });

        // Registra métricas de cache
        services.AddSingleton<CacheMetrics>();
        
        // Registra serviços de cache
        services.AddSingleton<ICacheService, HybridCacheService>();
        services.AddSingleton<ICacheWarmupService, CacheWarmupService>();

        return services;
    }
}