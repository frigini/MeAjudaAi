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

        // Redis como distributed cache (HybridCache usa automaticamente)
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "MeAjudaAi";
        });

        // Registra o serviço
        services.AddScoped<ICacheService, HybridCacheService>();

        return services;
    }
}