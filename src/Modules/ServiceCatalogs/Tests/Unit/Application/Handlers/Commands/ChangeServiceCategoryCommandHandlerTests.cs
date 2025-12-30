using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class ChangeServiceCategoryCommandHandlerTests
{
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly Mock<IServiceCategoryRepository> _categoryRepositoryMock;
    private readonly ChangeServiceCategoryCommandHandler _handler;

    public ChangeServiceCategoryCommandHandlerTests()
    {
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _categoryRepositoryMock = new Mock<IServiceCategoryRepository>();
        _handler = new ChangeServiceCategoryCommandHandler(_serviceRepositoryMock.Object, _categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var oldCategory = new ServiceCategoryBuilder().AsActive().Build();
        var newCategory = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(oldCategory.Id)
            .WithName("Limpeza de Piscina")
            .Build();
        var command = new ChangeServiceCategoryCommand(service.Id.Value, newCategory.Id.Value);

        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCategory);

        _serviceRepositoryMock
            .Setup(x => x.ExistsWithNameAsync(service.Name, It.IsAny<ServiceId>(), It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _serviceRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        service.CategoryId.Should().Be(newCategory.Id);
        _serviceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var command = new ChangeServiceCategoryCommand(serviceId, categoryId);

        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Serviço").And.Contain("não encontrado");
        _serviceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldReturnFailure()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Limpeza de Piscina")
            .Build();
        var newCategoryId = Guid.NewGuid();
        var command = new ChangeServiceCategoryCommand(service.Id.Value, newCategoryId);

        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceCategory?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .WithMessage("*não encontrada*");
        _serviceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInactiveCategory_ShouldReturnFailure()
    {
        // Arrange
        var oldCategory = new ServiceCategoryBuilder().AsActive().Build();
        var newCategory = new ServiceCategoryBuilder().AsInactive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(oldCategory.Id)
            .WithName("Limpeza de Piscina")
            .Build();
        var command = new ChangeServiceCategoryCommand(service.Id.Value, newCategory.Id.Value);

        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCategory);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .WithMessage("*inativa*");
        _serviceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDuplicateNameInTargetCategory_ShouldReturnFailure()
    {
        // Arrange
        var oldCategory = new ServiceCategoryBuilder().AsActive().Build();
        var newCategory = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(oldCategory.Id)
            .WithName("Limpeza de Piscina")
            .Build();
        var command = new ChangeServiceCategoryCommand(service.Id.Value, newCategory.Id.Value);

        _serviceRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCategory);

        _serviceRepositoryMock
            .Setup(x => x.ExistsWithNameAsync(service.Name, It.IsAny<ServiceId>(), It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Já existe");
        _serviceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyServiceId_ShouldReturnFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new ChangeServiceCategoryCommand(Guid.Empty, categoryId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("ID do serviço").And.Contain("não pode ser vazio");
        _serviceRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyCategoryId_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var command = new ChangeServiceCategoryCommand(serviceId, Guid.Empty);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("ID da nova categoria").And.Contain("não pode ser vazio");
        _serviceRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
