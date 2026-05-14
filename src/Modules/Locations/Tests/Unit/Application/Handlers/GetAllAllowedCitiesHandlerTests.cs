using FluentAssertions;
using MeAjudaAi.Modules.Locations.Application.Handlers;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Application.Handlers;

public class GetAllAllowedCitiesHandlerTests
{
    private readonly Mock<IAllowedCityQueries> _queriesMock;
    private readonly GetAllAllowedCitiesHandler _handler;

    public GetAllAllowedCitiesHandlerTests()
    {
        _queriesMock = new Mock<IAllowedCityQueries>();
        _handler = new GetAllAllowedCitiesHandler(_queriesMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithOnlyActiveTrue_ShouldReturnOnlyActiveCities()
    {
        // Arrange
        var query = new GetAllAllowedCitiesQuery { OnlyActive = true };
        var activeCities = new List<AllowedCity>
        {
            new("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0)
        };

        _queriesMock.Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeCities);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().CityName.Should().Be("Muriaé");
        _queriesMock.Verify(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithOnlyActiveFalse_ShouldReturnAllCities()
    {
        // Arrange
        var query = new GetAllAllowedCitiesQuery { OnlyActive = false };
        var allCities = new List<AllowedCity>
        {
            new("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0),
            new("Itaperuna", "RJ", "admin@test.com", 3302270, 0, 0, 0, false)
        };

        _queriesMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allCities);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        _queriesMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNoCities_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllAllowedCitiesQuery { OnlyActive = true };
        _queriesMock.Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AllowedCity>());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldMapPropertiesToDto()
    {
        // Arrange
        var query = new GetAllAllowedCitiesQuery { OnlyActive = true };
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, -21.1308, -42.3689, 25.5);
        _queriesMock.Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AllowedCity> { city });

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        var dto = result.First();
        dto.CityName.Should().Be(city.CityName);
        dto.StateSigla.Should().Be(city.StateSigla);
        dto.IbgeCode.Should().Be(city.IbgeCode);
        dto.IsActive.Should().Be(city.IsActive);
        dto.CreatedBy.Should().Be(city.CreatedBy);
        dto.Latitude.Should().Be(city.Latitude);
        dto.Longitude.Should().Be(city.Longitude);
        dto.ServiceRadiusKm.Should().Be(city.ServiceRadiusKm);
    }
}
