namespace MeAjudaAi.Shared.Events;

[AttributeUsage(AttributeTargets.Class)]
public sealed class HighVolumeEventAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class)]
public sealed class CriticalEventAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class)]
public sealed class DedicatedTopicAttribute(string topicName) : Attribute
{
    public string TopicName { get; } = topicName;
}