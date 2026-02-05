using FluentAssertions;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Application.Handlers.Commands.Service;

public class DeleteServiceCommandHandlerTests
{
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly Mock<IProvidersModuleApi> _providersModuleApiMock;
    private readonly DeleteServiceCommandHandler _handler;

    public DeleteServiceCommandHandlerTests()
    {
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _providersModuleApiMock = new Mock<IProvidersModuleApi>();
        _handler = new DeleteServiceCommandHandler(_serviceRepositoryMock.Object, _providersModuleApiMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenServiceExistsAndNotUsed_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var service = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service.Create(ServiceCategoryId.From(Guid.NewGuid()), "Service Name", "Description");
        var serviceId = service.Id.Value;
        var command = new DeleteServiceCommand(serviceId);

        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _providersModuleApiMock.Setup(a => a.HasProvidersOfferingServiceAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MeAjudaAi.Contracts.Functional.Result<bool>.Success(false));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _serviceRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenServiceNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeleteServiceCommand(Guid.NewGuid());

        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service?)null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_WhenServiceUsedByProviders_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var command = new DeleteServiceCommand(serviceId);
        var service = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service.Create(ServiceCategoryId.From(Guid.NewGuid()), "Service Name", "Description");

        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _providersModuleApiMock.Setup(a => a.HasProvidersOfferingServiceAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MeAjudaAi.Contracts.Functional.Result<bool>.Success(true));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Não é possível excluir o serviço");
        result.Error.Message.Should().Contain("pois ele é oferecido por prestadores");
    }
}
