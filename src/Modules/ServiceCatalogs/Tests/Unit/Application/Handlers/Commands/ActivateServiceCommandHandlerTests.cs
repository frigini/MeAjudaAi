using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class ActivateServiceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Service, ServiceId>> _repositoryMock;
    private readonly ActivateServiceCommandHandler _handler;

    public ActivateServiceCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<Service, ServiceId>>();
        _uowMock.Setup(x => x.GetRepository<Service, ServiceId>()).Returns(_repositoryMock.Object);
        _handler = new ActivateServiceCommandHandler(_uowMock.Object, NullLogger<ActivateServiceCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Limpeza de Piscina")
            .AsInactive()
            .Build();
        var command = new ActivateServiceCommand(service.Id.Value);

        _repositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        service.IsActive.Should().BeTrue();
        _repositoryMock.Verify(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var command = new ActivateServiceCommand(serviceId);

        _repositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("não encontrado");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyId_ShouldReturnFailure()
    {
        // Arrange
        var command = new ActivateServiceCommand(Guid.Empty);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("não pode ser vazio");
        _repositoryMock.Verify(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithAlreadyActiveService_ShouldReturnSuccess()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Limpeza de Piscina")
            .AsActive()
            .Build();
        var command = new ActivateServiceCommand(service.Id.Value);

        _repositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        service.IsActive.Should().BeTrue();
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesThrows_ShouldReturnGenericFailure()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .AsInactive()
            .Build();
        var command = new ActivateServiceCommand(service.Id.Value);

        _repositoryMock
            .Setup(x => x.TryFindAsync(service.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failure"));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Ocorreu um erro inesperado ao ativar o serviço.");
    }
}
