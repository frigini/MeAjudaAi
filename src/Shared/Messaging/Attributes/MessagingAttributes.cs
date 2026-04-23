namespace MeAjudaAi.Shared.Messaging.Attributes;

/// <summary>
/// Indica que um evento deve ser enviado para um tópico/fila dedicada, 
/// evitando o problema do "vizinho barulhento" no barramento principal.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class DedicatedTopicAttribute(string? topicName = null) : Attribute
{
    public string? TopicName { get; } = topicName;
}

/// <summary>
/// Indica que um evento tem alto volume e deve ser processado com maior paralelismo.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class HighVolumeEventAttribute : Attribute
{
    public int MaxParallelism { get; }

    public HighVolumeEventAttribute(int maxParallelism = 50)
    {
        if (maxParallelism < 1)
            throw new ArgumentOutOfRangeException(nameof(maxParallelism), maxParallelism, "O paralelismo máximo deve ser pelo menos 1.");

        MaxParallelism = maxParallelism;
    }
}

/// <summary>
/// Indica que um evento é crítico e deve usar filas com maior garantia de persistência (ex: Quorum Queues).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class CriticalEventAttribute : Attribute
{
}
