using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class UpdateServiceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Service, ServiceId>> _serviceRepositoryMock;
    private readonly Mock<IServiceQueries> _serviceQueriesMock;
    private readonly UpdateServiceCommandHandler _handler;

    public UpdateServiceCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _serviceRepositoryMock = new Mock<IRepository<Service, ServiceId>>();
        _serviceQueriesMock = new Mock<IServiceQueries>();

        _uowMock.Setup(x => x.GetRepository<Service, ServiceId>()).Returns(_serviceRepositoryMock.Object);

        _handler = new UpdateServiceCommandHandler(_uowMock.Object, _serviceQueriesMock.Object, NullLogger<UpdateServiceCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var command = new UpdateServiceCommand(serviceId, "Updated Name", "Updated Description", 2);

        _serviceRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("NotFound");
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var categoryId = ServiceCategoryId.From(Guid.NewGuid());
        var service = Service.Create(categoryId, "Original", "Desc", 1);
        var command = new UpdateServiceCommand(service.Id.Value, "Updated Name", "Updated Desc", 2);

        _serviceRepositoryMock
            .Setup(x => x.TryFindAsync(service.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _serviceQueriesMock
            .Setup(x => x.ExistsWithNameAsync("Updated Name", service.Id, service.CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnFailure()
    {
        // Arrange
        var categoryId = ServiceCategoryId.From(Guid.NewGuid());
        var service = Service.Create(categoryId, "Original", "Desc", 1);
        var command = new UpdateServiceCommand(service.Id.Value, "Duplicate Name", "Desc", 1);

        _serviceRepositoryMock
            .Setup(x => x.TryFindAsync(service.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _serviceQueriesMock
            .Setup(x => x.ExistsWithNameAsync("Duplicate Name", service.Id, service.CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyId_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateServiceCommand(Guid.Empty, "Name", "Desc", 1);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _serviceRepositoryMock.Verify(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyName_ShouldReturnFailure()
    {
        // Arrange
        var categoryId = ServiceCategoryId.From(Guid.NewGuid());
        var service = Service.Create(categoryId, "Original", "Desc", 1);
        var command = new UpdateServiceCommand(service.Id.Value, "   ", "Desc", 1);

        _serviceRepositoryMock
            .Setup(x => x.TryFindAsync(service.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _serviceQueriesMock.Verify(x => x.ExistsWithNameAsync(It.IsAny<string>(), It.IsAny<ServiceId>(), It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesThrows_ShouldReturnGenericFailure()
    {
        // Arrange
        var categoryId = ServiceCategoryId.From(Guid.NewGuid());
        var service = Service.Create(categoryId, "Original", "Desc", 1);
        var command = new UpdateServiceCommand(service.Id.Value, "New Name", "Desc", 1);

        _serviceRepositoryMock
            .Setup(x => x.TryFindAsync(service.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _serviceQueriesMock
            .Setup(x => x.ExistsWithNameAsync("New Name", service.Id, service.CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Ocorreu um erro inesperado.");
    }
}
