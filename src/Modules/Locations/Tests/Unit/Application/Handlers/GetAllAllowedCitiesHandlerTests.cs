using FluentAssertions;
using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Modules.Locations.Application.Handlers;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Application.Handlers;

public class GetAllAllowedCitiesHandlerTests
{
    private readonly Mock<IAllowedCityRepository> _repositoryMock;
    private readonly GetAllAllowedCitiesHandler _handler;

    public GetAllAllowedCitiesHandlerTests()
    {
        _repositoryMock = new Mock<IAllowedCityRepository>();
        _handler = new GetAllAllowedCitiesHandler(_repositoryMock.Object);
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

        _repositoryMock.Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeCities);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().CityName.Should().Be("Muriaé");
        _repositoryMock.Verify(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()), Times.Once);
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

        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allCities);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        _repositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNoCities_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllAllowedCitiesQuery { OnlyActive = true };
        _repositoryMock.Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
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
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        _repositoryMock.Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
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
    }
}
