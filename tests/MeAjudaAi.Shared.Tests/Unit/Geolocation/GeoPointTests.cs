using MeAjudaAi.Shared.Geolocation;

namespace MeAjudaAi.Shared.Tests.Unit.Geolocation;

[Trait("Category", "Unit")]
[Trait("Component", "Shared")]
[Trait("Layer", "Domain")]
public class GeoPointTests
{
    [Fact]
    public void Constructor_WithValidCoordinates_ShouldCreateGeoPoint()
    {
        // Arrange & Act
        var geoPoint = new GeoPoint(-23.5505, -46.6333); // São Paulo

        // Assert
        geoPoint.Latitude.Should().Be(-23.5505);
        geoPoint.Longitude.Should().Be(-46.6333);
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(91, 0)]
    [InlineData(-100, 0)]
    [InlineData(100, 0)]
    public void Constructor_WithInvalidLatitude_ShouldThrowArgumentOutOfRangeException(double latitude, double longitude)
    {
        // Act
        Action act = () => _ = new GeoPoint(latitude, longitude);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("latitude")
            .WithMessage("*Latitude deve estar entre -90 e 90*");
    }

    [Theory]
    [InlineData(0, -181)]
    [InlineData(0, 181)]
    [InlineData(0, -200)]
    [InlineData(0, 200)]
    public void Constructor_WithInvalidLongitude_ShouldThrowArgumentOutOfRangeException(double latitude, double longitude)
    {
        // Act
        Action act = () => _ = new GeoPoint(latitude, longitude);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("longitude")
            .WithMessage("*Longitude deve estar entre -180 e 180*");
    }

    [Fact]
    public void Constructor_WithBoundaryLatitude_ShouldCreateGeoPoint()
    {
        // Arrange & Act
        var geoPointMin = new GeoPoint(-90, 0);
        var geoPointMax = new GeoPoint(90, 0);

        // Assert
        geoPointMin.Latitude.Should().Be(-90);
        geoPointMax.Latitude.Should().Be(90);
    }

    [Fact]
    public void Constructor_WithBoundaryLongitude_ShouldCreateGeoPoint()
    {
        // Arrange & Act
        var geoPointMin = new GeoPoint(0, -180);
        var geoPointMax = new GeoPoint(0, 180);

        // Assert
        geoPointMin.Longitude.Should().Be(-180);
        geoPointMax.Longitude.Should().Be(180);
    }

    [Fact]
    public void DistanceTo_WithSamePoint_ShouldReturnZero()
    {
        // Arrange
        var point1 = new GeoPoint(-23.5505, -46.6333);
        var point2 = new GeoPoint(-23.5505, -46.6333);

        // Act
        var distance = point1.DistanceTo(point2);

        // Assert
        distance.Should().Be(0);
    }

    [Fact]
    public void DistanceTo_BetweenSaoPauloAndRioDeJaneiro_ShouldReturnApproximately360km()
    {
        // Arrange - coordenadas de São Paulo e Rio de Janeiro
        var saoPaulo = new GeoPoint(-23.5505, -46.6333);
        var rioDeJaneiro = new GeoPoint(-22.9068, -43.1729);

        // Act
        var distance = saoPaulo.DistanceTo(rioDeJaneiro);

        // Assert - distância real é aproximadamente 360km
        distance.Should().BeApproximately(360, 50); // Permite tolerância de 50km
    }

    [Fact]
    public void DistanceTo_WithVeryClosePoints_ShouldReturnSmallDistance()
    {
        // Arrange - pontos a 1km de distância
        var point1 = new GeoPoint(-23.5505, -46.6333);
        var point2 = new GeoPoint(-23.5595, -46.6333); // ~1km ao norte

        // Act
        var distance = point1.DistanceTo(point2);

        // Assert
        distance.Should().BeGreaterThan(0);
        distance.Should().BeLessThan(2); // Menos de 2km
    }

    [Fact]
    public void DistanceTo_WithPointsOnEquator_ShouldCalculateCorrectly()
    {
        // Arrange
        var point1 = new GeoPoint(0, 0);
        var point2 = new GeoPoint(0, 10);

        // Act
        var distance = point1.DistanceTo(point2);

        // Assert
        distance.Should().BeGreaterThan(0);
        distance.Should().BeApproximately(1113, 100); // ~1113km por 10 graus no equador
    }

    [Fact]
    public void DistanceTo_IsSymmetric_ShouldReturnSameDistanceInBothDirections()
    {
        // Arrange
        var point1 = new GeoPoint(-23.5505, -46.6333);
        var point2 = new GeoPoint(-22.9068, -43.1729);

        // Act
        var distance1to2 = point1.DistanceTo(point2);
        var distance2to1 = point2.DistanceTo(point1);

        // Assert
        distance1to2.Should().Be(distance2to1);
    }

    [Fact]
    public void GeoPoint_AsRecord_ShouldSupportValueEquality()
    {
        // Arrange
        var point1 = new GeoPoint(-23.5505, -46.6333);
        var point2 = new GeoPoint(-23.5505, -46.6333);
        var point3 = new GeoPoint(-22.9068, -43.1729);

        // Act & Assert
        point1.Should().Be(point2);
        point1.Should().NotBe(point3);
        (point1 == point2).Should().BeTrue();
        (point1 == point3).Should().BeFalse();
    }

    [Fact]
    public void GeoPoint_WithDeconstruction_ShouldExposeCoordinates()
    {
        // Arrange
        var geoPoint = new GeoPoint(-23.5505, -46.6333);

        // Act
        var (latitude, longitude) = geoPoint;

        // Assert
        latitude.Should().Be(-23.5505);
        longitude.Should().Be(-46.6333);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-23.5505, -46.6333)] // São Paulo
    [InlineData(51.5074, -0.1278)] // Londres
    [InlineData(40.7128, -74.0060)] // Nova York
    [InlineData(-33.8688, 151.2093)] // Sydney
    public void Constructor_WithVariousValidCoordinates_ShouldSucceed(double latitude, double longitude)
    {
        // Act
        var geoPoint = new GeoPoint(latitude, longitude);

        // Assert
        geoPoint.Latitude.Should().Be(latitude);
        geoPoint.Longitude.Should().Be(longitude);
    }
}
