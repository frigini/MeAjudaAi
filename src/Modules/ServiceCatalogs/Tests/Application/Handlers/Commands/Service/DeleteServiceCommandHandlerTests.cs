using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Application.Handlers.Commands.Service;

public class DeleteServiceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service, ServiceId>> _repositoryMock;
    private readonly Mock<IProvidersModuleApi> _providersModuleApiMock;
    private readonly DeleteServiceCommandHandler _handler;

    public DeleteServiceCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service, ServiceId>>();
        _providersModuleApiMock = new Mock<IProvidersModuleApi>();
        
        _uowMock.Setup(u => u.GetRepository<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service, ServiceId>())
            .Returns(_repositoryMock.Object);
            
        _handler = new DeleteServiceCommandHandler(
            _uowMock.Object, 
            _providersModuleApiMock.Object,
            NullLogger<DeleteServiceCommandHandler>.Instance);
    }

    [Fact]
    public async Task HandleAsync_WhenServiceExistsAndNotUsed_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var service = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service.Create(ServiceCategoryId.From(Guid.NewGuid()), "Service Name", "Description");
        var serviceId = service.Id.Value;
        var command = new DeleteServiceCommand(serviceId);

        _repositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _providersModuleApiMock.Setup(a => a.HasProvidersOfferingServiceAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MeAjudaAi.Contracts.Functional.Result<bool>.Success(false));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.Delete(It.IsAny<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenServiceNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeleteServiceCommand(Guid.NewGuid());

        _repositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
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

        _repositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _providersModuleApiMock.Setup(a => a.HasProvidersOfferingServiceAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MeAjudaAi.Contracts.Functional.Result<bool>.Success(true));

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain(string.Format(ValidationMessages.Catalogs.CannotDeleteServiceOffered, service.Name));
    }
}



