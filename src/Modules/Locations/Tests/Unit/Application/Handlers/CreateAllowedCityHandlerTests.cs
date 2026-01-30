using MeAjudaAi.Modules.Locations.Application.Handlers;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Modules.Locations.Application.Commands;
using FluentAssertions;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Application.Handlers;

public class CreateAllowedCityHandlerTests
{
    private readonly Mock<IAllowedCityRepository> _repositoryMock;
    private readonly Mock<IGeocodingService> _geocodingServiceMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly CreateAllowedCityHandler _handler;

    public CreateAllowedCityHandlerTests()
    {
        _repositoryMock = new Mock<IAllowedCityRepository>();
        _geocodingServiceMock = new Mock<IGeocodingService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _handler = new CreateAllowedCityHandler(_repositoryMock.Object, _geocodingServiceMock.Object, _httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateAllowedCityAndReturnId()
    {
        // Arrange
        var command = new CreateAllowedCityCommand("Muriaé", "MG", 3143906, 0, 0, 0, true);
        var userEmail = "admin@test.com";

        SetupHttpContext(userEmail);
        _repositoryMock.Setup(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<AllowedCity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCityAlreadyExists_ShouldThrowDuplicateAllowedCityException()
    {
        // Arrange
        var command = new CreateAllowedCityCommand("Muriaé", "MG", 3143906, 0, 0, 0, true);
        SetupHttpContext("admin@test.com");

        _repositoryMock.Setup(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DuplicateAllowedCityException>()
            .WithMessage("*já cadastrada*");
    }

    [Fact]
    public async Task HandleAsync_WithNoUserEmail_ShouldUseSystemAsCreator()
    {
        // Arrange
        var command = new CreateAllowedCityCommand("Muriaé", "MG", 3143906, 0, 0, 0, true);
        SetupHttpContext(null);

        _repositoryMock.Setup(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        AllowedCity? capturedCity = null;
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<AllowedCity>(), It.IsAny<CancellationToken>()))
            .Callback<AllowedCity, CancellationToken>((city, _) => capturedCity = city)
            .Returns(Task.CompletedTask);

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
        var command = new CreateAllowedCityCommand("Muriaé", "MG", null, 0, 0, 0, true);
        SetupHttpContext("admin@test.com");

        _repositoryMock.Setup(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        AllowedCity? capturedCity = null;
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<AllowedCity>(), It.IsAny<CancellationToken>()))
            .Callback<AllowedCity, CancellationToken>((city, _) => capturedCity = city)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedCity.Should().NotBeNull();
        capturedCity!.IbgeCode.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithIsActiveFalse_ShouldCreateInactiveCity()
    {
        // Arrange
        var command = new CreateAllowedCityCommand("Muriaé", "MG", 3143906, 0, 0, 0, false);
        SetupHttpContext("admin@test.com");

        _repositoryMock.Setup(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        AllowedCity? capturedCity = null;
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<AllowedCity>(), It.IsAny<CancellationToken>()))
            .Callback<AllowedCity, CancellationToken>((city, _) => capturedCity = city)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedCity.Should().NotBeNull();
        capturedCity!.IsActive.Should().BeFalse();
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
