using Microsoft.Extensions.Caching.Hybrid;

namespace MeAjudaAi.Shared.Caching;

public sealed class CacheOptions
{
    public TimeSpan? Expiration { get; init; }
    public TimeSpan? LocalCacheExpiration { get; init; }

    public HybridCacheEntryOptions ToHybridCacheEntryOptions()
    {
        if (Expiration == null && LocalCacheExpiration == null)
            return new HybridCacheEntryOptions();

        var options = new HybridCacheEntryOptions
        {
            Expiration = Expiration ?? TimeSpan.FromMinutes(15),
            LocalCacheExpiration = LocalCacheExpiration ?? TimeSpan.FromMinutes(5)
        };

        return options;
    }

    public static CacheOptions Default => new()
    {
        Expiration = TimeSpan.FromMinutes(15),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };

    public static CacheOptions ShortTerm => new()
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(1)
    };

    public static CacheOptions LongTerm => new()
    {
        Expiration = TimeSpan.FromHours(1),
        LocalCacheExpiration = TimeSpan.FromMinutes(15)
    };
}
