using System.Text.Json;

namespace MeAjudaAi.Shared.Serialization;

public sealed class SystemTextJsonSerializer(JsonSerializerOptions options) : ISerializer
{
    public string Serialize<T>(T obj) => JsonSerializer.Serialize(obj, options);

    public T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, options);
}
