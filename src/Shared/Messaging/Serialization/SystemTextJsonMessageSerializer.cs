using System.Text.Json;

namespace MeAjudaAi.Shared.Messaging.Serialization;

public sealed class SystemTextJsonMessageSerializer : IMessageSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public string Serialize<T>(T obj) => JsonSerializer.Serialize(obj, Options);

    public T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
}
