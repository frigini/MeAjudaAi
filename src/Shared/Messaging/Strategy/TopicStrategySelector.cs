using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.ServiceBus;
using System.Reflection;

namespace MeAjudaAi.Shared.Messaging.Strategy;

public class TopicStrategySelector(ServiceBusOptions serviceBusOptions) : ITopicStrategySelector
{
    private readonly ServiceBusOptions _serviceBusOptions = serviceBusOptions;

    public string SelectTopicForEvent<T>() => SelectTopicForEvent(typeof(T));

    public string SelectTopicForEvent(Type eventType)
    {
        // Verifica se tem tópico dedicado
        var dedicatedAttr = eventType.GetCustomAttribute<DedicatedTopicAttribute>();
        if (dedicatedAttr != null)
            return dedicatedAttr.TopicName;

        var domain = GetDomainFromEventType(eventType);

        return _serviceBusOptions.Strategy switch
        {
            ETopicStrategy.SingleWithFilters => _serviceBusOptions.DefaultTopicName,
            ETopicStrategy.MultipleByDomain => _serviceBusOptions.GetTopicForDomain(domain),
            ETopicStrategy.Hybrid => IsHighVolumeOrCritical(eventType)
                ? _serviceBusOptions.GetTopicForDomain(domain)
                : _serviceBusOptions.DefaultTopicName,
            _ => _serviceBusOptions.DefaultTopicName
        };
    }

    private static string GetDomainFromEventType(Type eventType)
    {
        var namespaceParts = eventType.Namespace?.Split('.') ?? [];
        return namespaceParts.Length > 3 ? namespaceParts[2] : "Shared";
    }

    private static bool IsHighVolumeOrCritical(Type eventType) =>
        eventType.GetCustomAttribute<HighVolumeEventAttribute>() != null ||
        eventType.GetCustomAttribute<CriticalEventAttribute>() != null;
}