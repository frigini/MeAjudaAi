using System.Text.Json;
using System.Text.Json.Serialization;
using MeAjudaAi.Shared.Geolocation;

namespace MeAjudaAi.Shared.Serialization.Converters;

public sealed class GeoPointConverter : JsonConverter<GeoPoint>
{
    public override GeoPoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        return new GeoPoint(
            root.GetProperty("latitude").GetDouble(),
            root.GetProperty("longitude").GetDouble()
        );
    }

    public override void Write(Utf8JsonWriter writer, GeoPoint value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("latitude", value.Latitude);
        writer.WriteNumber("longitude", value.Longitude);
        writer.WriteEndObject();
    }
}
