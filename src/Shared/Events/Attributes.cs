using System.Diagnostics.CodeAnalysis;
namespace MeAjudaAi.Shared.Events;

[AttributeUsage(AttributeTargets.Class)]
[ExcludeFromCodeCoverage]
public sealed class HighVolumeEventAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class)]
[ExcludeFromCodeCoverage]
public sealed class CriticalEventAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class)]
[ExcludeFromCodeCoverage]
public sealed class DedicatedTopicAttribute(string topicName) : Attribute
{
    public string TopicName { get; } = topicName;
}
