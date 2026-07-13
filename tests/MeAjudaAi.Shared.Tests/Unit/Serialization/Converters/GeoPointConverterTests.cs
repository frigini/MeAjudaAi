using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Serialization.Converters;
using System.Text.Json;

namespace MeAjudaAi.Shared.Tests.Unit.Serialization.Converters;

public class GeoPointConverterTests
{
    private readonly JsonSerializerOptions _options;

    public GeoPointConverterTests()
    {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new GeoPointConverter());
    }

    [Fact]
    public void Serialize_ShouldReturnCorrectJson()
    {
        // Arrange
        var point = new GeoPoint(-23.5505, -46.6333);

        // Act
        var json = JsonSerializer.Serialize(point, _options);

        // Assert
        json.Should().Contain("\"latitude\":-23.5505");
        json.Should().Contain("\"longitude\":-46.6333");
    }

    [Fact]
    public void Deserialize_ShouldReturnCorrectObject()
    {
        // Arrange
        var json = "{\"latitude\": -23.5505, \"longitude\": -46.6333}";

        // Act
        var result = JsonSerializer.Deserialize<GeoPoint>(json, _options);

        // Assert
        result.Should().NotBeNull();
        result!.Latitude.Should().Be(-23.5505);
        result.Longitude.Should().Be(-46.6333);
    }
}
