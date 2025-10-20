using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Events;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Shared.Messaging;

public class EventTypeRegistry(ICacheService cache, ILogger<EventTypeRegistry> logger) : IEventTypeRegistry
{
    private const string CacheKey = "event-types-registry";

    public async Task<IEnumerable<Type>> GetAllEventTypesAsync(CancellationToken cancellationToken = default)
    {
        var eventTypes = await cache.GetOrCreateAsync(
            CacheKey,
            async _ => await DiscoverEventTypesAsync(),
            expiration: TimeSpan.FromHours(1),
            tags: ["event-registry"],
            cancellationToken: cancellationToken);

        return eventTypes.Values;
    }

    public async Task<Type?> GetEventTypeAsync(string eventName, CancellationToken cancellationToken = default)
    {
        var eventTypes = await cache.GetOrCreateAsync(
            CacheKey,
            async _ => await DiscoverEventTypesAsync(),
            expiration: TimeSpan.FromHours(1),
            tags: ["event-registry"],
            cancellationToken: cancellationToken);

        return eventTypes.GetValueOrDefault(eventName);
    }

    private ValueTask<Dictionary<string, Type>> DiscoverEventTypesAsync()
    {
        logger.LogInformation("Discovering event types...");

        var eventTypes = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.StartsWith("MeAjudaAi") == true)
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IntegrationEvent).IsAssignableFrom(t) &&
                       !t.IsAbstract && t.IsPublic)
            .ToDictionary(t => t.Name, t => t);

        logger.LogInformation("Discovered {Count} event types", eventTypes.Count);
        return ValueTask.FromResult(eventTypes);
    }

    public async Task InvalidateCacheAsync(CancellationToken cancellationToken = default)
    {
        await cache.RemoveByPatternAsync("event-registry", cancellationToken);
    }
}
