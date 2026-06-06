using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using Microsoft.Extensions.Logging.Abstractions;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class DeleteServiceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Service, ServiceId>> _serviceRepositoryMock;
    private readonly Mock<IProvidersModuleApi> _providersModuleApiMock;
    private readonly DeleteServiceCommandHandler _handler;

    public DeleteServiceCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _serviceRepositoryMock = new Mock<IRepository<Service, ServiceId>>();
        _providersModuleApiMock = new Mock<IProvidersModuleApi>();

        _uowMock.Setup(x => x.GetRepository<Service, ServiceId>()).Returns(_serviceRepositoryMock.Object);

        _handler = new DeleteServiceCommandHandler(_uowMock.Object, _providersModuleApiMock.Object, NullLogger<DeleteServiceCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WithValidCommandAndNoProviders_ShouldReturnSuccess()
    {
        // Arrange
        var service = new ServiceBuilder().Build();
        var command = new DeleteServiceCommand(service.Id.Value);

        _serviceRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _providersModuleApiMock
            .Setup(x => x.HasProvidersOfferingServiceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(false));

        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _serviceRepositoryMock.Verify(x => x.Delete(It.IsAny<Service>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithProvidersModuleApiFailure_ShouldReturnFailure()
    {
        // Arrange
        var service = new ServiceBuilder().Build();
        var command = new DeleteServiceCommand(service.Id.Value);

        _serviceRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _providersModuleApiMock
            .Setup(x => x.HasProvidersOfferingServiceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Failure(new Error(ValidationMessages.Providers.ErrorRetrievingProviders)));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be(ValidationMessages.Providers.ErrorRetrievingProviders);
        _serviceRepositoryMock.Verify(x => x.Delete(It.IsAny<Service>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

        _serviceRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _providersModuleApiMock
            .Setup(x => x.HasProvidersOfferingServiceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be(string.Format(ValidationMessages.Catalogs.CannotDeleteServiceOffered, service.Name));
        _serviceRepositoryMock.Verify(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Once);
        _providersModuleApiMock.Verify(x => x.HasProvidersOfferingServiceAsync(service.Id.Value, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(x => x.Delete(It.IsAny<Service>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var command = new DeleteServiceCommand(serviceId);

        _serviceRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be(ValidationMessages.NotFound.Service);
        _providersModuleApiMock.Verify(x => x.HasProvidersOfferingServiceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _serviceRepositoryMock.Verify(x => x.Delete(It.IsAny<Service>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
        _serviceRepositoryMock.Verify(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Never);
        _providersModuleApiMock.Verify(x => x.HasProvidersOfferingServiceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _serviceRepositoryMock.Verify(x => x.Delete(It.IsAny<Service>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithRepositoryException_ShouldReturnFailure()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Limpeza de Piscina")
            .Build();
        var command = new DeleteServiceCommand(service.Id.Value);

        _serviceRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _providersModuleApiMock
            .Setup(x => x.HasProvidersOfferingServiceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(false));

        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Ocorreu um erro inesperado ao excluir o serviço.");
    }
}



