using MeAjudaAi.Shared.Geolocation;

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
