using FluentAssertions;
using MeAjudaAi.Modules.Locations.Application.Handlers;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Application.Handlers;

public class GetAllowedCityByIdHandlerTests
{
    private readonly Mock<IAllowedCityQueries> _queriesMock;
    private readonly GetAllowedCityByIdHandler _handler;

    public GetAllowedCityByIdHandlerTests()
    {
        _queriesMock = new Mock<IAllowedCityQueries>();
        _handler = new GetAllowedCityByIdHandler(_queriesMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidId_ShouldReturnAllowedCityDto()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var query = new GetAllowedCityByIdQuery { Id = cityId };
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, -21.130, -42.366, 15);

        _queriesMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CityName.Should().Be("Muriaé");
        result.StateSigla.Should().Be("MG");
        result.IbgeCode.Should().Be(3143906);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var query = new GetAllowedCityByIdQuery { Id = Guid.NewGuid() };

        _queriesMock.Setup(x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AllowedCity?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithInactiveCity_ShouldReturnDto()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var query = new GetAllowedCityByIdQuery { Id = cityId };
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0, false);

        _queriesMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_ShouldMapAllPropertiesToDto()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var query = new GetAllowedCityByIdQuery { Id = cityId };
        var city = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);

        _queriesMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CityName.Should().Be(city.CityName);
        result.StateSigla.Should().Be(city.StateSigla);
        result.IbgeCode.Should().Be(city.IbgeCode);
        result.IsActive.Should().Be(city.IsActive);
        result.CreatedAt.Should().Be(city.CreatedAt);
        result.CreatedBy.Should().Be(city.CreatedBy);
        result.Latitude.Should().Be(city.Latitude);
        result.Longitude.Should().Be(city.Longitude);
        result.ServiceRadiusKm.Should().Be(city.ServiceRadiusKm);
    }
}
