using FluentAssertions;
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
    public async Task Handle_WithOnlyActiveTrue_ShouldReturnOnlyActiveCities()
    {
        // Arrange
        var activeCity = new AllowedCity("Muriaé", "MG", "3143906", "admin@test.com");
        var cities = new List<AllowedCity> { activeCity };

        var query = new GetAllAllowedCitiesQuery { OnlyActive = true };

        _repositoryMock.Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cities);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().CityName.Should().Be("Muriaé");
        result.First().IsActive.Should().BeTrue();

        _repositoryMock.Verify(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithOnlyActiveFalse_ShouldReturnAllCities()
    {
        // Arrange
        var activeCity = new AllowedCity("Muriaé", "MG", "3143906", "admin@test.com");
        var inactiveCity = new AllowedCity("Itaperuna", "RJ", "3302270", "admin@test.com");
        inactiveCity.Deactivate("admin@test.com");

        var cities = new List<AllowedCity> { activeCity, inactiveCity };

        var query = new GetAllAllowedCitiesQuery { OnlyActive = false };

        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cities);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(dto => dto.CityName == "Muriaé" && dto.IsActive);
        result.Should().Contain(dto => dto.CityName == "Itaperuna" && !dto.IsActive);

        _repositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenNoCities_ShouldReturnEmptyList()
    {
        // Arrange
        var cities = new List<AllowedCity>();
        var query = new GetAllAllowedCitiesQuery { OnlyActive = true };

        _repositoryMock.Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cities);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();

        _repositoryMock.Verify(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var city = new AllowedCity("Muriaé", "MG", "3143906", "admin@test.com");
        typeof(AllowedCity).GetProperty("Id")!.SetValue(city, cityId);

        var cities = new List<AllowedCity> { city };
        var query = new GetAllAllowedCitiesQuery { OnlyActive = true };

        _repositoryMock.Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cities);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.First();
        dto.Id.Should().Be(cityId);
        dto.CityName.Should().Be("Muriaé");
        dto.StateSigla.Should().Be("MG");
        dto.IbgeCode.Should().Be("3143906");
        dto.IsActive.Should().BeTrue();
        dto.CreatedBy.Should().Be("admin@test.com");
        dto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        dto.UpdatedAt.Should().BeNull();
        dto.UpdatedBy.Should().BeNull();
    }
}
