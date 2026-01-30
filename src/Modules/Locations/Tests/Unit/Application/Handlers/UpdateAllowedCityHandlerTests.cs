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

public class UpdateAllowedCityHandlerTests
{
    private readonly Mock<IAllowedCityRepository> _repositoryMock;
    private readonly Mock<IGeocodingService> _geocodingServiceMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly UpdateAllowedCityHandler _handler;

    public UpdateAllowedCityHandlerTests()
    {
        _repositoryMock = new Mock<IAllowedCityRepository>();
        _geocodingServiceMock = new Mock<IGeocodingService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _handler = new UpdateAllowedCityHandler(_repositoryMock.Object, _geocodingServiceMock.Object, _httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldUpdateAllowedCity()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCity = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
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
        var userEmail = "admin2@test.com";

        SetupHttpContext(userEmail);
        _repositoryMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);
        _repositoryMock.Setup(x => x.GetByCityAndStateAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AllowedCity?)null);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        existingCity.CityName.Should().Be("Itaperuna");
        existingCity.StateSigla.Should().Be("RJ");
        existingCity.IbgeCode.Should().Be(3302270);
        _repositoryMock.Verify(x => x.UpdateAsync(existingCity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCityNotFound_ShouldThrowAllowedCityNotFoundException()
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
        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AllowedCity?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(404);
        result.Error.Message.Should().Contain("não encontrada");
    }

    [Fact]
    public async Task HandleAsync_WhenDuplicateCityExists_ShouldThrowDuplicateAllowedCityException()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCity = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        var differentCityId = Guid.NewGuid();
        var duplicateCity = new AllowedCity("Itaperuna", "RJ", "admin@test.com", 3302270, 0, 0, 0);
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
        _repositoryMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);
        _repositoryMock.Setup(x => x.GetByCityAndStateAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(duplicateCity);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DuplicateAllowedCityException>()
            .WithMessage("*já cadastrada*");
    }

    [Fact]
    public async Task HandleAsync_UpdatingSameCityWithSameName_ShouldNotThrowException()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCity = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
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
        _repositoryMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.UpdateAsync(existingCity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNoUserEmail_ShouldUseSystemAsUpdater()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCity = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
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
        _repositoryMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);
        _repositoryMock.Setup(x => x.GetByCityAndStateAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
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
