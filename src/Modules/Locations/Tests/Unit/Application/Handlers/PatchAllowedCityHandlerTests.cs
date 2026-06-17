using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.Handlers.Commands;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Locations;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Application.Handlers;

public class PatchAllowedCityHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<AllowedCity, Guid>> _repositoryMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly PatchAllowedCityHandler _handler;

    public PatchAllowedCityHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<AllowedCity, Guid>>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

        _uowMock.Setup(x => x.GetRepository<AllowedCity, Guid>()).Returns(_repositoryMock.Object);

        _handler = new PatchAllowedCityHandler(_uowMock.Object, _httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task HandleAsync_UpdateRadius_ShouldUpdateServiceRadiusOK()
    {
        var cityId = Guid.NewGuid();
        var existingCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG")
            .WithCreatedBy("test@user.com")
            .WithIbgeCode(3143906)
            .WithCoordinates(-21.1, -42.3)
            .WithServiceRadius(10)
            .Build();
        var command = new PatchAllowedCityCommand(cityId, ServiceRadiusKm: 50, IsActive: null);

        SetupHttpContext("admin@test.com");
        _repositoryMock.Setup(x => x.TryFindAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        existingCity.ServiceRadiusKm.Should().Be(50);
        existingCity.UpdatedBy.Should().Be("admin@test.com");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ActivateCity_ShouldSetIsActiveTrue()
    {
        var cityId = Guid.NewGuid();
        var existingCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG")
            .WithCreatedBy("test@user.com")
            .WithIbgeCode(3143906)
            .WithCoordinates(-21.1, -42.3)
            .WithServiceRadius(10)
            .AsInactive()
            .Build();
        var command = new PatchAllowedCityCommand(cityId, ServiceRadiusKm: null, IsActive: true);

        SetupHttpContext("admin@test.com");
        _repositoryMock.Setup(x => x.TryFindAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        existingCity.IsActive.Should().BeTrue();
        existingCity.UpdatedBy.Should().Be("admin@test.com");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DeactivateCity_ShouldSetIsActiveFalse()
    {
        var cityId = Guid.NewGuid();
        var existingCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG")
            .WithCreatedBy("test@user.com")
            .WithIbgeCode(3143906)
            .WithCoordinates(-21.1, -42.3)
            .WithServiceRadius(10)
            .AsActive()
            .Build();
        var command = new PatchAllowedCityCommand(cityId, ServiceRadiusKm: null, IsActive: false);

        SetupHttpContext("admin@test.com");
        _repositoryMock.Setup(x => x.TryFindAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        existingCity.IsActive.Should().BeFalse();
        existingCity.UpdatedBy.Should().Be("admin@test.com");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CityNotFound_ShouldReturnFailure()
    {
        var command = new PatchAllowedCityCommand(Guid.NewGuid(), ServiceRadiusKm: 50, IsActive: null);

        SetupHttpContext("admin@test.com");
        _repositoryMock.Setup(x => x.TryFindAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AllowedCity?)null);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_NoUserContext_ShouldUseSystemUser()
    {
        var cityId = Guid.NewGuid();
        var existingCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG")
            .WithCreatedBy("test@user.com")
            .WithIbgeCode(3143906)
            .WithCoordinates(-21.1, -42.3)
            .WithServiceRadius(10)
            .Build();
        var command = new PatchAllowedCityCommand(cityId, ServiceRadiusKm: 50, IsActive: null);

        SetupHttpContext(null);
        _repositoryMock.Setup(x => x.TryFindAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        existingCity.UpdatedBy.Should().Be("system");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NoChanges_ShouldReturnSuccessWithoutSave()
    {
        var cityId = Guid.NewGuid();
        var existingCity = AllowedCityBuilder.AsTestCity("Muriaé", "MG")
            .WithCreatedBy("test@user.com")
            .WithIbgeCode(3143906)
            .WithCoordinates(-21.1, -42.3)
            .WithServiceRadius(10)
            .Build();
        var command = new PatchAllowedCityCommand(cityId, ServiceRadiusKm: null, IsActive: null);

        SetupHttpContext("admin@test.com");
        _repositoryMock.Setup(x => x.TryFindAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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


