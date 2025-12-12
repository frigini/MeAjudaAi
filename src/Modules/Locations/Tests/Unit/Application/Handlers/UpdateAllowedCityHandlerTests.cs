using FluentAssertions;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.Handlers;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Application.Handlers;

public class UpdateAllowedCityHandlerTests
{
    private readonly Mock<IAllowedCityRepository> _repositoryMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly UpdateAllowedCityHandler _handler;

    public UpdateAllowedCityHandlerTests()
    {
        _repositoryMock = new Mock<IAllowedCityRepository>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _handler = new UpdateAllowedCityHandler(_repositoryMock.Object, _httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateCity()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCity = new AllowedCity("Muriaé", "MG", "3143906", "admin@test.com");
        typeof(AllowedCity).GetProperty("Id")!.SetValue(existingCity, cityId);

        var command = new UpdateAllowedCityCommand
        {
            Id = cityId,
            CityName = "Itaperuna",
            StateSigla = "RJ",
            IbgeCode = "3302270",
            IsActive = true
        };

        var userEmail = "admin2@test.com";
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, userEmail)
        }));

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        _repositoryMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);
        _repositoryMock.Setup(x => x.GetByCityAndStateAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AllowedCity?)null);
        _repositoryMock.Setup(x => x.UpdateAsync(existingCity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingCity.CityName.Should().Be(command.CityName);
        existingCity.StateSigla.Should().Be(command.StateSigla);
        existingCity.IbgeCode.Should().Be(command.IbgeCode);
        existingCity.IsActive.Should().Be(command.IsActive);
        existingCity.UpdatedBy.Should().Be(userEmail);
        existingCity.UpdatedAt.Should().NotBeNull();

        _repositoryMock.Verify(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(existingCity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCityNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var command = new UpdateAllowedCityCommand
        {
            Id = cityId,
            CityName = "Muriaé",
            StateSigla = "MG",
            IbgeCode = "3143906",
            IsActive = true
        };

        var userEmail = "admin@test.com";
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, userEmail)
        }));

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        _repositoryMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AllowedCity?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*não encontrada*");

        _repositoryMock.Verify(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<AllowedCity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDuplicateCityExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCity = new AllowedCity("Muriaé", "MG", "3143906", "admin@test.com");
        typeof(AllowedCity).GetProperty("Id")!.SetValue(existingCity, cityId);

        var duplicateCity = new AllowedCity("Itaperuna", "RJ", "3302270", "admin@test.com");
        typeof(AllowedCity).GetProperty("Id")!.SetValue(duplicateCity, Guid.NewGuid());

        var command = new UpdateAllowedCityCommand
        {
            Id = cityId,
            CityName = "Itaperuna",
            StateSigla = "RJ",
            IbgeCode = "3302270",
            IsActive = true
        };

        var userEmail = "admin2@test.com";
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, userEmail)
        }));

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        _repositoryMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);
        _repositoryMock.Setup(x => x.GetByCityAndStateAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(duplicateCity);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*já cadastrada*");

        _repositoryMock.Verify(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<AllowedCity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSameCityIsUpdated_ShouldAllowUpdate()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCity = new AllowedCity("Muriaé", "MG", "3143906", "admin@test.com");
        typeof(AllowedCity).GetProperty("Id")!.SetValue(existingCity, cityId);

        var command = new UpdateAllowedCityCommand
        {
            Id = cityId,
            CityName = "Muriaé",
            StateSigla = "MG",
            IbgeCode = "3143906",
            IsActive = false
        };

        var userEmail = "admin2@test.com";
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, userEmail)
        }));

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        _repositoryMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);
        _repositoryMock.Setup(x => x.GetByCityAndStateAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);
        _repositoryMock.Setup(x => x.UpdateAsync(existingCity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingCity.IsActive.Should().BeFalse();
        existingCity.UpdatedBy.Should().Be(userEmail);

        _repositoryMock.Verify(x => x.UpdateAsync(existingCity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutUserEmail_ShouldUseSystemAsUpdatedBy()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCity = new AllowedCity("Muriaé", "MG", "3143906", "admin@test.com");
        typeof(AllowedCity).GetProperty("Id")!.SetValue(existingCity, cityId);

        var command = new UpdateAllowedCityCommand
        {
            Id = cityId,
            CityName = "Itaperuna",
            StateSigla = "RJ",
            IbgeCode = "3302270",
            IsActive = true
        };

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        _repositoryMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);
        _repositoryMock.Setup(x => x.GetByCityAndStateAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AllowedCity?)null);
        _repositoryMock.Setup(x => x.UpdateAsync(existingCity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        existingCity.UpdatedBy.Should().Be("system");

        _repositoryMock.Verify(x => x.UpdateAsync(existingCity, It.IsAny<CancellationToken>()), Times.Once);
    }
}
