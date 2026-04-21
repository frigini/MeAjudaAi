using System.Reflection;
using MeAjudaAi.Shared.Messaging.Attributes;
using Rebus.Topic;

namespace MeAjudaAi.Shared.Messaging.Rebus.Conventions;

/// <summary>
/// Convenção de nomes de tópicos que interpreta o atributo [DedicatedTopic]
/// </summary>
public class AttributeTopicNameConvention(ITopicNameConvention fallback) : ITopicNameConvention
{
    public string GetTopic(Type eventType)
    {
        var attribute = eventType.GetCustomAttribute<DedicatedTopicAttribute>();
        
        if (attribute != null && !string.IsNullOrWhiteSpace(attribute.TopicName))
        {
            return attribute.TopicName;
        }

        // Delega para o fallback (convenção original do Rebus ou transporte)
        return fallback.GetTopic(eventType);
    }
}
