using FluentAssertions;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.Handlers;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Modules.Locations.Domain.Entities;
using MeAjudaAi.Modules.Locations.Domain.Exceptions;
using MeAjudaAi.Shared.Database;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Locations.Tests.Unit.Application.Handlers;

public class DeleteAllowedCityHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IAllowedCityQueries> _queriesMock;
    private readonly Mock<IRepository<AllowedCity, Guid>> _repositoryMock;
    private readonly DeleteAllowedCityHandler _handler;

    public DeleteAllowedCityHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _queriesMock = new Mock<IAllowedCityQueries>();
        _repositoryMock = new Mock<IRepository<AllowedCity, Guid>>();
        
        _uowMock.Setup(x => x.GetRepository<AllowedCity, Guid>()).Returns(_repositoryMock.Object);
        
        _handler = new DeleteAllowedCityHandler(_uowMock.Object, _queriesMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidId_ShouldDeleteAllowedCity()
    {
        var cityId = Guid.NewGuid();
        var existingCity = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0);
        var command = new DeleteAllowedCityCommand { Id = cityId };

        _queriesMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);

        await _handler.HandleAsync(command, CancellationToken.None);

        _repositoryMock.Verify(x => x.Delete(existingCity), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCityNotFound_ShouldThrowAllowedCityNotFoundException()
    {
        var command = new DeleteAllowedCityCommand { Id = Guid.NewGuid() };

        _queriesMock.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AllowedCity?)null);

        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<AllowedCityNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task HandleAsync_WithInactiveCity_ShouldStillDelete()
    {
        var cityId = Guid.NewGuid();
        var existingCity = new AllowedCity("Muriaé", "MG", "admin@test.com", 3143906, 0, 0, 0, false);
        var command = new DeleteAllowedCityCommand { Id = cityId };

        _queriesMock.Setup(x => x.GetByIdAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCity);

        await _handler.HandleAsync(command, CancellationToken.None);

        _repositoryMock.Verify(x => x.Delete(existingCity), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}