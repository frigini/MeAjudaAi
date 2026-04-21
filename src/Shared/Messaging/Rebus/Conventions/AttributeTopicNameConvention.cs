using System.Reflection;
using MeAjudaAi.Shared.Messaging.Attributes;
using Rebus.Topic;

namespace MeAjudaAi.Shared.Messaging.Rebus.Conventions;

/// <summary>
/// Convenção de nomes de tópicos que interpreta o atributo [DedicatedTopic]
/// </summary>
public class AttributeTopicNameConvention : ITopicNameConvention
{
    public string GetTopic(Type eventType)
    {
        var attribute = eventType.GetCustomAttribute<DedicatedTopicAttribute>();
        
        if (attribute != null && !string.IsNullOrWhiteSpace(attribute.TopicName))
        {
            return attribute.TopicName;
        }

        // Fallback para o comportamento padrão do Rebus (FullName do tipo)
        return eventType.FullName ?? eventType.Name;
    }
}
