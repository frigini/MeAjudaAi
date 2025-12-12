using FluentAssertions;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.Handlers;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.Repositories;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Application.Handlers;

public class DeleteAllowedCityHandlerTests
{
    private readonly Mock<IAllowedCityRepository> _repositoryMock;
    private readonly DeleteAllowedCityHandler _handler;

    public DeleteAllowedCityHandlerTests()
    {
        _repositoryMock = new Mock<IAllowedCityRepository>();
        _handler = new DeleteAllowedCityHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldDeleteCity()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCity = new AllowedCity("Muriaé", "MG", "3143906", "admin@test.com");
        typeof(AllowedCity).GetProperty("Id")!.SetValue(existingCity, cityId);

        var command = new DeleteAllowedCityCommand { Id = cityId };

        _repositoryMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);
        _repositoryMock.Setup(x => x.DeleteAsync(existingCity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.DeleteAsync(existingCity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCityNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var command = new DeleteAllowedCityCommand { Id = cityId };

        _repositoryMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AllowedCity?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*não encontrada*");

        _repositoryMock.Verify(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<AllowedCity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCityIsInactive_ShouldStillDeleteCity()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCity = new AllowedCity("Muriaé", "MG", "3143906", "admin@test.com");
        typeof(AllowedCity).GetProperty("Id")!.SetValue(existingCity, cityId);
        existingCity.Deactivate("admin@test.com");

        var command = new DeleteAllowedCityCommand { Id = cityId };

        _repositoryMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);
        _repositoryMock.Setup(x => x.DeleteAsync(existingCity, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.DeleteAsync(existingCity, It.IsAny<CancellationToken>()), Times.Once);
    }
}
