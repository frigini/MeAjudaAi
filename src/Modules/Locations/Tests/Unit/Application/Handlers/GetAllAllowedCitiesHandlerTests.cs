using MeAjudaAi.Modules.Locations.Application.Handlers;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Locations;

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
        var query = new GetAllAllowedCitiesQuery { OnlyActive = true };
        var activeCities = new List<AllowedCity>
        {
            AllowedCityBuilder.AsTestCity("Muriaé", "MG").WithIbgeCode(3143906).Build()
        };

        _queriesMock.Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeCities);

        var result = await _handler.HandleAsync(query, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().CityName.Should().Be("Muriaé");
        _queriesMock.Verify(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithOnlyActiveFalse_ShouldReturnAllCities()
    {
        var query = new GetAllAllowedCitiesQuery { OnlyActive = false };
        var allCities = new List<AllowedCity>
        {
            AllowedCityBuilder.AsTestCity("Muriaé", "MG").WithIbgeCode(3143906).Build(),
            AllowedCityBuilder.AsTestCity("Itaperuna", "RJ").WithIbgeCode(3302270).AsInactive().Build()
        };

        _queriesMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allCities);

        var result = await _handler.HandleAsync(query, CancellationToken.None);

        result.Should().HaveCount(2);
        _queriesMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNoCities_ShouldReturnEmptyList()
    {
        var query = new GetAllAllowedCitiesQuery { OnlyActive = true };
        _queriesMock.Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AllowedCity>());

        var result = await _handler.HandleAsync(query, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldMapPropertiesToDto()
    {
        var query = new GetAllAllowedCitiesQuery { OnlyActive = true };
        var city = AllowedCityBuilder.AsTestCity("Muriaé", "MG")
            .WithIbgeCode(3143906)
            .WithCoordinates(-21.1308, -42.3689)
            .WithServiceRadius(25.5)
            .Build();
        _queriesMock.Setup(x => x.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AllowedCity> { city });

        var result = await _handler.HandleAsync(query, CancellationToken.None);

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