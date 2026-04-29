using System.Diagnostics.CodeAnalysis;
using MeAjudaAi.Shared.Messaging.Strategy;

namespace MeAjudaAi.Shared.Messaging.Options;

[ExcludeFromCodeCoverage]
public sealed class MessageBusOptions
{
    public TimeSpan DefaultTimeToLive { get; set; } = TimeSpan.FromDays(1);
    public int MaxConcurrentCalls { get; set; } = 1;
    public int MaxDeliveryCount { get; set; } = 10;
    public TimeSpan LockDuration { get; set; } = TimeSpan.FromMinutes(5);
    public bool EnableAutoDiscovery { get; set; } = true;
    public string[] AssemblyPrefixes { get; set; } = { "MeAjudaAi" };

    // Estratégia de tópicos: Single vs Multiple
    public ETopicStrategy Strategy { get; set; } = ETopicStrategy.SingleWithFilters;

    public Func<Type, string> QueueNamingConvention { get; set; } =
        type => type.Name.ToLowerInvariant();

    public Func<Type, string> TopicNamingConvention { get; set; } =
        type =>
        {
            if (string.IsNullOrEmpty(type.Namespace)) return "events.events";
            
            var nsSpan = type.Namespace.AsSpan();
            var lastDotIndex = nsSpan.LastIndexOf('.');
            
            var lastPart = lastDotIndex >= 0 
                ? nsSpan.Slice(lastDotIndex + 1) 
                : nsSpan;
                
            return $"{lastPart.ToString().ToLowerInvariant()}.events";
        };

    public Func<Type, string> SubscriptionNamingConvention { get; set; } =
        type => Environment.MachineName.ToLowerInvariant();
}
