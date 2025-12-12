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

public class CreateAllowedCityHandlerTests
{
    private readonly Mock<IAllowedCityRepository> _repositoryMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly CreateAllowedCityHandler _handler;

    public CreateAllowedCityHandlerTests()
    {
        _repositoryMock = new Mock<IAllowedCityRepository>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _handler = new CreateAllowedCityHandler(_repositoryMock.Object, _httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateAndReturnCityId()
    {
        // Arrange
        var command = new CreateAllowedCityCommand
        {
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
        _repositoryMock.Setup(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        AllowedCity? capturedCity = null;
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<AllowedCity>(), It.IsAny<CancellationToken>()))
            .Callback<AllowedCity, CancellationToken>((city, _) => capturedCity = city)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedCity.Should().NotBeNull();
        capturedCity!.CityName.Should().Be(command.CityName);
        capturedCity.StateSigla.Should().Be(command.StateSigla);
        capturedCity.IbgeCode.Should().Be(command.IbgeCode);
        capturedCity.IsActive.Should().Be(command.IsActive);
        capturedCity.CreatedBy.Should().Be(userEmail);

        _repositoryMock.Verify(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<AllowedCity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCityAlreadyExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CreateAllowedCityCommand
        {
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
        _repositoryMock.Setup(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*já cadastrada*");

        _repositoryMock.Verify(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<AllowedCity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithoutUserEmail_ShouldUseSystemAsCreatedBy()
    {
        // Arrange
        var command = new CreateAllowedCityCommand
        {
            CityName = "Muriaé",
            StateSigla = "MG",
            IbgeCode = "3143906",
            IsActive = true
        };

        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        _repositoryMock.Setup(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        AllowedCity? capturedCity = null;
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<AllowedCity>(), It.IsAny<CancellationToken>()))
            .Callback<AllowedCity, CancellationToken>((city, _) => capturedCity = city)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedCity.Should().NotBeNull();
        capturedCity!.CreatedBy.Should().Be("system");

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<AllowedCity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullIbgeCode_ShouldCreateCity()
    {
        // Arrange
        var command = new CreateAllowedCityCommand
        {
            CityName = "Muriaé",
            StateSigla = "MG",
            IbgeCode = null,
            IsActive = true
        };

        var userEmail = "admin@test.com";
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, userEmail)
        }));

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        _repositoryMock.Setup(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        AllowedCity? capturedCity = null;
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<AllowedCity>(), It.IsAny<CancellationToken>()))
            .Callback<AllowedCity, CancellationToken>((city, _) => capturedCity = city)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedCity.Should().NotBeNull();
        capturedCity!.IbgeCode.Should().BeNull();

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<AllowedCity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_WithDifferentIsActiveValues_ShouldCreateCityWithCorrectStatus(bool isActive)
    {
        // Arrange
        var command = new CreateAllowedCityCommand
        {
            CityName = "Muriaé",
            StateSigla = "MG",
            IbgeCode = "3143906",
            IsActive = isActive
        };

        var userEmail = "admin@test.com";
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, userEmail)
        }));

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
        _repositoryMock.Setup(x => x.ExistsAsync(command.CityName, command.StateSigla, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        AllowedCity? capturedCity = null;
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<AllowedCity>(), It.IsAny<CancellationToken>()))
            .Callback<AllowedCity, CancellationToken>((city, _) => capturedCity = city)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        capturedCity.Should().NotBeNull();
        capturedCity!.IsActive.Should().Be(isActive);

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<AllowedCity>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
