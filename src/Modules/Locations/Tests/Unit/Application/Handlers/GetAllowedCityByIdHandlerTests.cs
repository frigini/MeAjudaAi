using FluentAssertions;
using MeAjudaAi.Modules.Locations.Application.Handlers;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Application.Handlers;

public class GetAllowedCityByIdHandlerTests
{
    private readonly Mock<IAllowedCityRepository> _repositoryMock;
    private readonly GetAllowedCityByIdHandler _handler;

    public GetAllowedCityByIdHandlerTests()
    {
        _repositoryMock = new Mock<IAllowedCityRepository>();
        _handler = new GetAllowedCityByIdHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnCity()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var city = new AllowedCity("Muriaé", "MG", "3143906", "admin@test.com");
        typeof(AllowedCity).GetProperty("Id")!.SetValue(city, cityId);

        var query = new GetAllowedCityByIdQuery { Id = cityId };

        _repositoryMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(cityId);
        result.CityName.Should().Be("Muriaé");
        result.StateSigla.Should().Be("MG");
        result.IbgeCode.Should().Be("3143906");
        result.IsActive.Should().BeTrue();
        result.CreatedBy.Should().Be("admin@test.com");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.UpdatedAt.Should().BeNull();
        result.UpdatedBy.Should().BeNull();

        _repositoryMock.Verify(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCityNotFound_ShouldReturnNull()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var query = new GetAllowedCityByIdQuery { Id = cityId };

        _repositoryMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AllowedCity?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _repositoryMock.Verify(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInactiveCity_ShouldReturnCity()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var city = new AllowedCity("Muriaé", "MG", "3143906", "admin@test.com");
        typeof(AllowedCity).GetProperty("Id")!.SetValue(city, cityId);
        city.Deactivate("admin@test.com");

        var query = new GetAllowedCityByIdQuery { Id = cityId };

        _repositoryMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();

        _repositoryMock.Verify(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var city = new AllowedCity("Muriaé", "MG", "3143906", "admin@test.com");
        typeof(AllowedCity).GetProperty("Id")!.SetValue(city, cityId);
        city.Update("Itaperuna", "RJ", "3302270", "admin2@test.com");

        var query = new GetAllowedCityByIdQuery { Id = cityId };

        _repositoryMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(city);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(cityId);
        result.CityName.Should().Be("Itaperuna");
        result.StateSigla.Should().Be("RJ");
        result.IbgeCode.Should().Be("3302270");
        result.CreatedBy.Should().Be("admin@test.com");
        result.UpdatedBy.Should().Be("admin2@test.com");
        result.UpdatedAt.Should().NotBeNull();
    }
}
