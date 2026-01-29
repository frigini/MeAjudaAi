using FluentAssertions;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Shared.Exceptions;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Application.Handlers.Commands.Service;

public class UpdateServiceCommandHandlerTests
{
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly UpdateServiceCommandHandler _handler;

    public UpdateServiceCommandHandlerTests()
    {
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _handler = new UpdateServiceCommandHandler(_serviceRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenIdIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateServiceCommand(Guid.Empty, "Name", "Desc", 1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(ValidationMessages.Required.Id);
    }

    [Fact]
    public async Task HandleAsync_WhenServiceNotFound_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var command = new UpdateServiceCommand(serviceId, "Name", "Desc", 1);

        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(ValidationMessages.NotFound.Service);
    }

    [Fact]
    public async Task HandleAsync_WhenNameIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var command = new UpdateServiceCommand(serviceId, "", "Desc", 1);

        var service = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service.Create(
            new ServiceCategoryId(Guid.NewGuid()),
            "Original Name",
            "Original Desc",
            0);
        
        // HACK: ID is generated inside Create, need to verify if we can mock return with correct ID or just match by Any
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(ValidationMessages.Required.ServiceName);
    }

    [Fact]
    public async Task HandleAsync_WhenNameExists_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var command = new UpdateServiceCommand(serviceId, "New Name", "Desc", 1);

        var service = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service.Create(
            new ServiceCategoryId(Guid.NewGuid()),
            "Original Name",
            "Original Desc",
            0);

        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _serviceRepositoryMock.Setup(r => r.ExistsWithNameAsync(It.IsAny<string>(), It.IsAny<ServiceId>(), It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain(string.Format(ValidationMessages.Catalogs.ServiceNameExists, "New Name"));
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var command = new UpdateServiceCommand(serviceId, "Updated Name", "Updated Desc", 2);

        var service = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service.Create(
            new ServiceCategoryId(Guid.NewGuid()),
            "Original Name",
            "Original Desc",
            0);

        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _serviceRepositoryMock.Setup(r => r.ExistsWithNameAsync(It.IsAny<string>(), It.IsAny<ServiceId>(), It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        service.Name.Should().Be("Updated Name");
        service.Description.Should().Be("Updated Desc");
        service.DisplayOrder.Should().Be(2);

        _serviceRepositoryMock.Verify(r => r.UpdateAsync(service, It.IsAny<CancellationToken>()), Times.Once);
    }
}
