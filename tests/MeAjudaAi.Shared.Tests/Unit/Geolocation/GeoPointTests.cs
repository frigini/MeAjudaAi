using System.Text.Json;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Serialization.Converters;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Geolocation;

[Trait("Category", "Unit")]
public class GeoPointTests
{
    [Fact]
    public void Constructor_WithValidCoordinates_ShouldCreatePoint()
    {
        // Act
        var point = new GeoPoint(-23.5505, -46.6333); // São Paulo

        // Assert
        point.Latitude.Should().Be(-23.5505);
        point.Longitude.Should().Be(-46.6333);
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(91, 0)]
    [InlineData(0, -181)]
    [InlineData(0, 181)]
    public void Constructor_WithInvalidCoordinates_ShouldThrowArgumentOutOfRangeException(double lat, double lon)
    {
        // Act & Assert
        Action act = () => _ = new GeoPoint(lat, lon);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void DistanceTo_ShouldCalculateCorrectly()
    {
        // Arrange
        var sp = new GeoPoint(-23.5505, -46.6333);
        var rj = new GeoPoint(-22.9068, -43.1729);

        // Act
        var distance = sp.DistanceTo(rj);

        // Assert
        // São Paulo to Rio is roughly 350-360km
        distance.Should().BeInRange(350, 370);
    }

    [Fact]
    public void Deconstruct_ShouldReturnLatitudeAndLongitude()
    {
        // Arrange
        var point = new GeoPoint(-23.5505, -46.6333);

        // Act
        var (lat, lon) = point;

        // Assert
        lat.Should().Be(-23.5505);
        lon.Should().Be(-46.6333);
    }
}

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
