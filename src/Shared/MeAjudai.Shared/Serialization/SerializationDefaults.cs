using MeAjudaAi.Shared.Serialization.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MeAjudaAi.Shared.Serialization;

public static class SerializationDefaults
{
    public static JsonSerializerOptions Default => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            new GeoPointConverter()
        }
    };

    public static JsonSerializerOptions Api => new(Default)
    {
        WriteIndented = false
    };

    public static JsonSerializerOptions Logging => new(Default)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };
}