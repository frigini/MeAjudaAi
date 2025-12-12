using FluentAssertions;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.Handlers;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.Exceptions;
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
    public async Task HandleAsync_WithValidId_ShouldDeleteAllowedCity()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCity = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906);
        var command = new DeleteAllowedCityCommand { Id = cityId };

        _repositoryMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.DeleteAsync(existingCity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCityNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new DeleteAllowedCityCommand { Id = Guid.NewGuid() };

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AllowedCity?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AllowedCityNotFoundException>()
            .WithMessage("*não encontrada*");
    }

    [Fact]
    public async Task HandleAsync_WithInactiveCity_ShouldStillDelete()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var existingCity = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, false);
        var command = new DeleteAllowedCityCommand { Id = cityId };

        _repositoryMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.DeleteAsync(existingCity, It.IsAny<CancellationToken>()), Times.Once);
    }
}
