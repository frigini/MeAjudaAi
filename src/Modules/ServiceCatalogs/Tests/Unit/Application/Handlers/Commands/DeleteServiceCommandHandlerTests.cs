using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;

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
// ...
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
        result.Error!.Message.Should().Be(string.Format(ValidationMessages.Catalogs.CannotDeleteServiceOffered, service.Name));
        _repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Once);
        _providersModuleApiMock.Verify(x => x.HasProvidersOfferingServiceAsync(service.Id.Value, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Never);
    }
// ...
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
        result.Error!.Message.Should().Be(ValidationMessages.NotFound.Service);
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
        result.Error!.Message.Should().Be(ValidationMessages.Required.Id);
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
