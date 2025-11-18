using MeAjudaAi.Modules.Catalogs.Application.Commands;
using MeAjudaAi.Modules.Catalogs.Application.Handlers.Commands;
using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.Catalogs.Tests.Builders;

namespace MeAjudaAi.Modules.Catalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Catalogs")]
[Trait("Layer", "Application")]
public class CreateServiceCommandHandlerTests
{
    private readonly Mock<IServiceCategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly CreateServiceCommandHandler _handler;

    public CreateServiceCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IServiceCategoryRepository>();
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _handler = new CreateServiceCommandHandler(_serviceRepositoryMock.Object, _categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var command = new CreateServiceCommand(category.Id.Value, "Limpeza de Piscina", "Limpeza profunda", 1);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _serviceRepositoryMock
            .Setup(x => x.ExistsWithNameAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _serviceRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        _serviceRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldReturnFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateServiceCommand(categoryId, "Service Name", "Description", 1);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceCategory?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("not found");
        _serviceRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInactiveCategory_ShouldReturnFailure()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsInactive().Build();
        var command = new CreateServiceCommand(category.Id.Value, "Limpeza de Piscina", "Limpeza profunda", 1);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("inactive");
        _serviceRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnFailure()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var command = new CreateServiceCommand(category.Id.Value, "Duplicate Name", "Description", 1);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _serviceRepositoryMock
            .Setup(x => x.ExistsWithNameAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("already exists");
        _serviceRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
