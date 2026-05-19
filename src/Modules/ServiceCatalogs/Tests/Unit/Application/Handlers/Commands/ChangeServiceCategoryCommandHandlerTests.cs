using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Exceptions;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class ChangeServiceCategoryCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Service, ServiceId>> _serviceRepositoryMock;
    private readonly Mock<IServiceQueries> _serviceQueriesMock;
    private readonly Mock<IServiceCategoryQueries> _categoryQueriesMock;
    private readonly ChangeServiceCategoryCommandHandler _handler;

    public ChangeServiceCategoryCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _serviceRepositoryMock = new Mock<IRepository<Service, ServiceId>>();
        _serviceQueriesMock = new Mock<IServiceQueries>();
        _categoryQueriesMock = new Mock<IServiceCategoryQueries>();

        _uowMock.Setup(x => x.GetRepository<Service, ServiceId>()).Returns(_serviceRepositoryMock.Object);

        _handler = new ChangeServiceCategoryCommandHandler(_uowMock.Object, _serviceQueriesMock.Object, _categoryQueriesMock.Object);
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
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCategory);

        _serviceQueriesMock
            .Setup(x => x.ExistsWithNameAsync(service.Name, It.IsAny<ServiceId>(), It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        service.CategoryId.Should().Be(newCategory.Id);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var command = new ChangeServiceCategoryCommand(serviceId, categoryId);

        _serviceRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Serviço").And.Contain("não encontrado");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceCategory?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .WithMessage("*não encontrada*");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCategory);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .WithMessage("*inativa*");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCategory);

        _serviceQueriesMock
            .Setup(x => x.ExistsWithNameAsync(service.Name, It.IsAny<ServiceId>(), It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Já existe");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
        _serviceRepositoryMock.Verify(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Never);
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
        _serviceRepositoryMock.Verify(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
