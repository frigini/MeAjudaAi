using System.Text.Json.Serialization;

namespace MeAjudaAi.Contracts.Shared;

/// <summary>
/// Prioridade de entrega de uma comunicação.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ECommunicationPriority
{
    Low = 0,
    Normal = 1,
    High = 2
}
