using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Contracts.Modules.Providers;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class DeleteServiceCommandHandlerTests
{
    private readonly Mock<IServiceRepository> _repositoryMock;
    private readonly Mock<IProvidersModuleApi> _providersModuleApiMock;
    private readonly DeleteServiceCommandHandler _handler;

    public DeleteServiceCommandHandlerTests()
    {
        _repositoryMock = new Mock<IServiceRepository>();
        _providersModuleApiMock = new Mock<IProvidersModuleApi>();
        _handler = new DeleteServiceCommandHandler(_repositoryMock.Object, _providersModuleApiMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommandAndNoProviders_ShouldReturnSuccess()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Limpeza de Piscina")
            .Build();
        var command = new DeleteServiceCommand(service.Id.Value);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _providersModuleApiMock
            .Setup(x => x.HasProvidersOfferingServiceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(false));

        _repositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Once);
        _providersModuleApiMock.Verify(x => x.HasProvidersOfferingServiceAsync(service.Id.Value, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithServiceBeingOfferedByProviders_ShouldReturnFailure()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Limpeza de Piscina")
            .Build();
        var command = new DeleteServiceCommand(service.Id.Value);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _providersModuleApiMock
            .Setup(x => x.HasProvidersOfferingServiceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Cannot delete service");
        result.Error!.Message.Should().Contain("being offered by one or more providers");
        result.Error!.Message.Should().Contain("deactivate the service instead");
        _repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Once);
        _providersModuleApiMock.Verify(x => x.HasProvidersOfferingServiceAsync(service.Id.Value, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithProvidersModuleApiFailure_ShouldReturnFailure()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Limpeza de Piscina")
            .Build();
        var command = new DeleteServiceCommand(service.Id.Value);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _providersModuleApiMock
            .Setup(x => x.HasProvidersOfferingServiceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Failure("Providers module is unavailable"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Failed to verify if providers offer this service");
        result.Error!.Message.Should().Contain("Providers module is unavailable");
        _repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Once);
        _providersModuleApiMock.Verify(x => x.HasProvidersOfferingServiceAsync(service.Id.Value, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var command = new DeleteServiceCommand(serviceId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("not found");
        _providersModuleApiMock.Verify(x => x.HasProvidersOfferingServiceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyId_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeleteServiceCommand(Guid.Empty);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("cannot be empty");
        _repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Never);
        _providersModuleApiMock.Verify(x => x.HasProvidersOfferingServiceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithRepositoryException_ShouldPropagateException()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Limpeza de Piscina")
            .Build();
        var command = new DeleteServiceCommand(service.Id.Value);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _providersModuleApiMock
            .Setup(x => x.HasProvidersOfferingServiceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(false));

        _repositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _handler.HandleAsync(command, CancellationToken.None));
    }
}
