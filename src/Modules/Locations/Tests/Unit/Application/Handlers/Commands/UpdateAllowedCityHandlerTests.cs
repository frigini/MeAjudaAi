using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.Handlers.Commands;
using MeAjudaAi.Modules.Locations.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Locations;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Application.Handlers.Commands;

public class UpdateAllowedCityHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IAllowedCityQueries> _queriesMock;
    private readonly Mock<IGeocodingService> _geocodingServiceMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ILogger<UpdateAllowedCityHandler>> _loggerMock;
    private readonly UpdateAllowedCityHandler _handler;

    public UpdateAllowedCityHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _queriesMock = new Mock<IAllowedCityQueries>();
        _geocodingServiceMock = new Mock<IGeocodingService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _loggerMock = new Mock<ILogger<UpdateAllowedCityHandler>>();

        _geocodingServiceMock.Setup(x => x.GetCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeoPoint(0, 0));

        _handler = new UpdateAllowedCityHandler(_uowMock.Object, _queriesMock.Object, _geocodingServiceMock.Object, _loggerMock.Object, _httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldUpdateAllowedCity()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG")
            .WithIbgeCode(3143906)
            .Build();
        var command = new UpdateAllowedCityCommand
        {
            Id = cityId,
            CityName = "Itaperuna",
            StateSigla = "RJ",
            IbgeCode = 3302270,
            Latitude = 10,
            Longitude = 20,
            ServiceRadiusKm = 15,
            IsActive = true
        };
        var userEmail = "admin2@test.com";

        SetupHttpContext(userEmail);
        _queriesMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);
        _queriesMock.Setup(x => x.GetByCityAndStateAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AllowedCity?)null);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        existingCity.CityName.Should().Be("Itaperuna");
        existingCity.StateSigla.Should().Be("RJ");
        existingCity.IbgeCode.Should().Be(3302270);
        existingCity.Latitude.Should().Be(10);
        existingCity.Longitude.Should().Be(20);
        existingCity.ServiceRadiusKm.Should().Be(15);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCityNotFound_ShouldReturnNotFoundResult()
    {
        // Arrange
        var command = new UpdateAllowedCityCommand
        {
            Id = Guid.NewGuid(),
            CityName = "Itaperuna",
            StateSigla = "RJ",
            IbgeCode = 3302270,
            Latitude = 0,
            Longitude = 0,
            ServiceRadiusKm = 0,
            IsActive = true
        };

        SetupHttpContext("admin@test.com");
        _queriesMock.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AllowedCity?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(404);
        result.Error.Message.Should().Contain("não encontrada");
    }

    [Fact]
    public async Task HandleAsync_WhenDuplicateCityExists_ShouldReturnConflictError()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG")
            .WithIbgeCode(3143906)
            .Build();
        var differentCityId = Guid.NewGuid();
        var duplicateCity = AllowedCityBuilder.AsTestCity("Itaperuna", "RJ")
            .WithIbgeCode(3302270)
            .Build();
        var command = new UpdateAllowedCityCommand
        {
            Id = cityId,
            CityName = "Itaperuna",
            StateSigla = "RJ",
            IbgeCode = 3302270,
            Latitude = 0,
            Longitude = 0,
            ServiceRadiusKm = 0,
            IsActive = true
        };

        SetupHttpContext("admin@test.com");
        _queriesMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);
        _queriesMock.Setup(x => x.GetByCityAndStateAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(duplicateCity);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(409);
        result.Error.Message.Should().Contain("já cadastrada");
    }

    [Fact]
    public async Task HandleAsync_WhenCityNameChanged_AndCoordsMissing_ShouldCallGeocodingService()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG")
            .WithIbgeCode(3143906)
            .WithCoordinates(10, 20)
            .Build();
        var command = new UpdateAllowedCityCommand
        {
            Id = cityId,
            CityName = "New City",
            StateSigla = "MG",
            IbgeCode = 3143906,
            Latitude = 0,
            Longitude = 0,
            ServiceRadiusKm = 0,
            IsActive = true
        };

        SetupHttpContext("admin@test.com");
        _queriesMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);
        _queriesMock.Setup(x => x.GetByCityAndStateAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AllowedCity?)null);

        var expectedCoords = new GeoPoint(30, 40);
        _geocodingServiceMock.Setup(x => x.GetCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCoords);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _geocodingServiceMock.Verify(x => x.GetCoordinatesAsync($"{command.CityName}, {command.StateSigla}, Brasil", It.IsAny<CancellationToken>()), Times.Once);
        existingCity.Latitude.Should().Be(expectedCoords.Latitude);
        existingCity.Longitude.Should().Be(expectedCoords.Longitude);
    }

    [Fact]
    public async Task HandleAsync_WhenStateChanged_AndCoordsMissing_ShouldCallGeocodingService()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG")
            .WithIbgeCode(3143906)
            .WithCoordinates(10, 20)
            .Build();
        var command = new UpdateAllowedCityCommand
        {
            Id = cityId,
            CityName = "Muriaé",
            StateSigla = "RJ",
            IbgeCode = 3143906,
            Latitude = 0,
            Longitude = 0,
            ServiceRadiusKm = 0,
            IsActive = true
        };

        SetupHttpContext("admin@test.com");
        _queriesMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);
        _queriesMock.Setup(x => x.GetByCityAndStateAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AllowedCity?)null);

        var expectedCoords = new GeoPoint(30, 40);
        _geocodingServiceMock.Setup(x => x.GetCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCoords);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _geocodingServiceMock.Verify(x => x.GetCoordinatesAsync($"{command.CityName}, {command.StateSigla}, Brasil", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenExistingCoordsInvalid_AndCommandCoordsMissing_ShouldCallGeocodingService()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG")
            .WithIbgeCode(3143906)
            .Build();
        var command = new UpdateAllowedCityCommand
        {
            Id = cityId,
            CityName = "Muriaé",
            StateSigla = "MG",
            IbgeCode = 3143906,
            Latitude = 0,
            Longitude = 0,
            ServiceRadiusKm = 0,
            IsActive = true
        };

        SetupHttpContext("admin@test.com");
        _queriesMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);

        var expectedCoords = new GeoPoint(30, 40);
        _geocodingServiceMock.Setup(x => x.GetCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCoords);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _geocodingServiceMock.Verify(x => x.GetCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        existingCity.Latitude.Should().Be(expectedCoords.Latitude);
        existingCity.Longitude.Should().Be(expectedCoords.Longitude);
    }

    [Fact]
    public async Task HandleAsync_WhenGeocodingFails_ShouldKeepExistingCoordinates()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCoordsLat = 10.5;
        var existingCoordsLon = 20.5;
        var existingCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG")
            .WithIbgeCode(3143906)
            .WithCoordinates(existingCoordsLat, existingCoordsLon)
            .Build();
        var command = new UpdateAllowedCityCommand
        {
            Id = cityId,
            CityName = "New City",
            StateSigla = "MG",
            IbgeCode = 3143906,
            Latitude = 0,
            Longitude = 0,
            ServiceRadiusKm = 0,
            IsActive = true
        };

        SetupHttpContext("admin@test.com");
        _queriesMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);
        _queriesMock.Setup(x => x.GetByCityAndStateAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AllowedCity?)null);

        _geocodingServiceMock.Setup(x => x.GetCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Geocoding service unavailable"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existingCity.Latitude.Should().Be(existingCoordsLat);
        existingCity.Longitude.Should().Be(existingCoordsLon);
    }

    [Fact]
    public async Task HandleAsync_UpdatingSameCityWithSameName_ShouldNotThrowException()
    {
        // Arrange
        var existingCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG")
            .WithIbgeCode(3143906)
            .Build();
        var cityId = existingCity.Id;
        var command = new UpdateAllowedCityCommand
        {
            Id = cityId,
            CityName = "Muriaé",
            StateSigla = "MG",
            IbgeCode = 3143906,
            Latitude = 0,
            Longitude = 0,
            ServiceRadiusKm = 0,
            IsActive = true
        };

        SetupHttpContext("admin@test.com");
        _queriesMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);
        _queriesMock.Setup(x => x.GetByCityAndStateAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNoUserEmail_ShouldUseSystemAsUpdater()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG")
            .WithIbgeCode(3143906)
            .Build();
        var command = new UpdateAllowedCityCommand
        {
            Id = cityId,
            CityName = "Itaperuna",
            StateSigla = "RJ",
            IbgeCode = 3302270,
            Latitude = 0,
            Longitude = 0,
            ServiceRadiusKm = 0,
            IsActive = true
        };

        SetupHttpContext(null);
        _queriesMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);
        _queriesMock.Setup(x => x.GetByCityAndStateAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AllowedCity?)null);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        existingCity.UpdatedBy.Should().Be("system");
    }

    private void SetupHttpContext(string? userEmail)
    {
        var claims = userEmail != null
            ? new List<Claim> { new(ClaimTypes.Email, userEmail) }
            : new List<Claim>();

        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
    }
}