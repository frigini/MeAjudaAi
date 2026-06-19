using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.Handlers.Commands;
using MeAjudaAi.Modules.Locations.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Application.Handlers.Commands;

public class CreateAllowedCityHandlerTests
{
    private readonly Mock<IGeocodingService> _geocodingServiceMock;
    private readonly CreateAllowedCityHandler _handler;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ILogger<CreateAllowedCityHandler>> _loggerMock;
    private readonly Mock<IAllowedCityQueries> _queriesMock;
    private readonly Mock<IRepository<AllowedCity, Guid>> _repositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;

    public CreateAllowedCityHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _queriesMock = new Mock<IAllowedCityQueries>();
        _geocodingServiceMock = new Mock<IGeocodingService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _loggerMock = new Mock<ILogger<CreateAllowedCityHandler>>();
        _repositoryMock = new Mock<IRepository<AllowedCity, Guid>>();

        _uowMock.Setup(x => x.GetRepository<AllowedCity, Guid>()).Returns(_repositoryMock.Object);

        _handler = new CreateAllowedCityHandler(
            _uowMock.Object,
            _queriesMock.Object,
            _geocodingServiceMock.Object,
            _httpContextAccessorMock.Object,
            _loggerMock.Object);
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

    [Fact]
    public async Task HandleAsync_WhenCityAlreadyExists_ShouldReturnConflictError()
    {
        // Arrange
        var command = new CreateAllowedCityCommand("Muriaé", "MG", 3143906);
        SetupHttpContext("admin@test.com");

        _queriesMock.Setup(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(409);
        result.Error.Message.Should().Contain("já cadastrada");
    }

    [Fact]
    public async Task HandleAsync_WhenGeocodingReturnsCoordinates_ShouldUseGeocodedCoordinates()
    {
        // Arrange
        var command = new CreateAllowedCityCommand("Muriaé", "MG", 3143906);
        var geoPoint = new GeoPoint(-21.1311, -42.3708);
        SetupHttpContext("admin@test.com");

        _queriesMock.Setup(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _geocodingServiceMock
            .Setup(x => x.GetCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(geoPoint);

        AllowedCity? capturedCity = null;
        _repositoryMock.Setup(x => x.Add(It.IsAny<AllowedCity>()))
            .Callback<AllowedCity>(city => capturedCity = city);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedCity.Should().NotBeNull();
        capturedCity!.Latitude.Should().Be(geoPoint.Latitude);
        capturedCity.Longitude.Should().Be(geoPoint.Longitude);
    }

    [Fact]
    public async Task HandleAsync_WhenGeocodingThrows_ShouldStillCreateCity()
    {
        // Arrange
        var command = new CreateAllowedCityCommand("Muriaé", "MG", 3143906, -21.1, -42.3, 10, true);
        SetupHttpContext("admin@test.com");

        _queriesMock.Setup(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _geocodingServiceMock
            .Setup(x => x.GetCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Geocoding unavailable"));

        AllowedCity? capturedCity = null;
        _repositoryMock.Setup(x => x.Add(It.IsAny<AllowedCity>()))
            .Callback<AllowedCity>(city => capturedCity = city);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedCity.Should().NotBeNull();
        capturedCity!.Latitude.Should().Be(command.Latitude);
        capturedCity.Longitude.Should().Be(command.Longitude);
    }

    [Fact]
    public async Task HandleAsync_WithIsActiveFalse_ShouldCreateInactiveCity()
    {
        // Arrange
        var command = new CreateAllowedCityCommand("Muriaé", "MG", 3143906, IsActive: false);
        SetupHttpContext("admin@test.com");

        _queriesMock.Setup(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _geocodingServiceMock
            .Setup(x => x.GetCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GeoPoint?)null);

        AllowedCity? capturedCity = null;
        _repositoryMock.Setup(x => x.Add(It.IsAny<AllowedCity>()))
            .Callback<AllowedCity>(city => capturedCity = city);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedCity.Should().NotBeNull();
        capturedCity!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_WithNoUserEmail_ShouldUseSystemAsCreator()
    {
        // Arrange
        var command = new CreateAllowedCityCommand("Muriaé", "MG", 3143906);
        SetupHttpContext(null);

        _queriesMock.Setup(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _geocodingServiceMock
            .Setup(x => x.GetCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GeoPoint?)null);

        AllowedCity? capturedCity = null;
        _repositoryMock.Setup(x => x.Add(It.IsAny<AllowedCity>()))
            .Callback<AllowedCity>(city => capturedCity = city);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedCity.Should().NotBeNull();
        capturedCity!.CreatedBy.Should().Be("system");
    }

    [Fact]
    public async Task HandleAsync_WithNullIbgeCode_ShouldCreateCity()
    {
        // Arrange
        var command = new CreateAllowedCityCommand("Muriaé", "MG", null);
        SetupHttpContext("admin@test.com");

        _queriesMock.Setup(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _geocodingServiceMock
            .Setup(x => x.GetCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GeoPoint?)null);

        AllowedCity? capturedCity = null;
        _repositoryMock.Setup(x => x.Add(It.IsAny<AllowedCity>()))
            .Callback<AllowedCity>(city => capturedCity = city);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedCity.Should().NotBeNull();
        capturedCity!.IbgeCode.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateAllowedCityAndReturnId()
    {
        // Arrange
        var command = new CreateAllowedCityCommand("Muriaé", "MG", 3143906);
        var userEmail = "admin@test.com";

        SetupHttpContext(userEmail);
        _queriesMock.Setup(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _geocodingServiceMock
            .Setup(x => x.GetCoordinatesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GeoPoint?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _repositoryMock.Verify(x => x.Add(It.IsAny<AllowedCity>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}