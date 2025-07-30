using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudai.Shared.Caching;

internal static class Extensions
{
    public static IServiceCollection AddCaching(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });
        services.AddScoped<ICacheService, HybridCacheService>();

        return services;
    }
}