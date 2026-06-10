using System.Text.Json;

namespace MeAjudaAi.Shared.Serialization;

public sealed class SystemTextJsonSerializer : ISerializer
{
    private readonly JsonSerializerOptions _options;

    public SystemTextJsonSerializer() : this(SerializationDefaults.Default) { }

    public SystemTextJsonSerializer(JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = new JsonSerializerOptions(options);
    }

    public string Serialize<T>(T obj) => JsonSerializer.Serialize(obj, _options);

    public T? Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json, _options);
}
