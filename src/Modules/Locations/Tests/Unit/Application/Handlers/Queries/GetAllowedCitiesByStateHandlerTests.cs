using MeAjudaAi.Modules.Locations.Application.Handlers;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Locations;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
public class GetAllowedCitiesByStateHandlerTests
{
    private readonly Mock<IAllowedCityQueries> _queriesMock;
    private readonly GetAllowedCitiesByStateHandler _handler;

    public GetAllowedCitiesByStateHandlerTests()
    {
        _queriesMock = new Mock<IAllowedCityQueries>();
        _handler = new GetAllowedCitiesByStateHandler(_queriesMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidState_ShouldReturnCitiesForThatState()
    {
        // Arrange
        var query = new GetAllowedCitiesByStateQuery { State = "MG" };
        var cities = new List<AllowedCity>
        {
            AllowedCityBuilder.AsTestCity("Muriaé", "MG").WithIbgeCode(3143906).Build(),
            AllowedCityBuilder.AsTestCity("Juiz de Fora", "MG").WithIbgeCode(3136702).Build()
        };

        _queriesMock.Setup(x => x.GetByStateAsync("MG", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cities);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(c => c.StateSigla == "MG");
        _queriesMock.Verify(x => x.GetByStateAsync("MG", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithStateHavingNoCities_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllowedCitiesByStateQuery { State = "AM" };
        _queriesMock.Setup(x => x.GetByStateAsync("AM", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AllowedCity>());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldMapAllPropertiesToDto()
    {
        // Arrange
        var query = new GetAllowedCitiesByStateQuery { State = "RJ" };
        var city = AllowedCityBuilder.AsTestCity("Niterói", "RJ")
            .WithIbgeCode(3303302)
            .WithCoordinates(-22.8833, -43.1036)
            .WithServiceRadius(20.5)
            .Build();

        _queriesMock.Setup(x => x.GetByStateAsync("RJ", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AllowedCity> { city });

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        var dto = result.First();
        dto.Id.Should().Be(city.Id);
        dto.CityName.Should().Be(city.CityName);
        dto.StateSigla.Should().Be(city.StateSigla);
        dto.IbgeCode.Should().Be(city.IbgeCode);
        dto.Latitude.Should().Be(city.Latitude);
        dto.Longitude.Should().Be(city.Longitude);
        dto.ServiceRadiusKm.Should().Be(city.ServiceRadiusKm);
        dto.IsActive.Should().Be(city.IsActive);
        dto.CreatedAt.Should().Be(city.CreatedAt);
        dto.CreatedBy.Should().Be(city.CreatedBy);
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCancellationTokenToQuery()
    {
        // Arrange
        var query = new GetAllowedCitiesByStateQuery { State = "SP" };
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _queriesMock.Setup(x => x.GetByStateAsync("SP", token))
            .ReturnsAsync(new List<AllowedCity>());

        // Act
        await _handler.HandleAsync(query, token);

        // Assert
        _queriesMock.Verify(x => x.GetByStateAsync("SP", token), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithInactiveCities_ShouldReturnAllCities()
    {
        // Arrange - GetByStateAsync returns all cities for state, active or not
        var query = new GetAllowedCitiesByStateQuery { State = "MG" };
        var cities = new List<AllowedCity>
        {
            AllowedCityBuilder.AsTestCity("Muriaé", "MG").AsActive().Build(),
            AllowedCityBuilder.AsTestCity("Belo Horizonte", "MG").AsInactive().Build()
        };

        _queriesMock.Setup(x => x.GetByStateAsync("MG", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cities);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First(c => c.CityName == "Muriaé").IsActive.Should().BeTrue();
        result.First(c => c.CityName == "Belo Horizonte").IsActive.Should().BeFalse();
    }
}
