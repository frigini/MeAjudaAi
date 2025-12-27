using System.Text.Json;
using System.Text.Json.Serialization;
using MeAjudaAi.Shared.Serialization.Converters;

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
            new StrictEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true),
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

    public static JsonSerializerOptions HealthChecks(bool isDevelopment = false) => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = isDevelopment
    };
}
