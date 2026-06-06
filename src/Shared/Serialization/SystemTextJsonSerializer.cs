using System.Text.Json;

namespace MeAjudaAi.Shared.Serialization;

public sealed class SystemTextJsonSerializer : ISerializer
{
    private readonly JsonSerializerOptions _options = SerializationDefaults.Default;

    public string Serialize<T>(T obj) => System.Text.Json.JsonSerializer.Serialize(obj, _options);

    public T? Deserialize<T>(string json) => System.Text.Json.JsonSerializer.Deserialize<T>(json, _options);
}
