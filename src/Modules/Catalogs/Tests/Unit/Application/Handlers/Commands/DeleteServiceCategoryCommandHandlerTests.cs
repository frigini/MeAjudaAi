using MeAjudaAi.Modules.Catalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.Catalogs.Application.Handlers.Commands.ServiceCategory;
using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.Catalogs.Tests.Builders;

namespace MeAjudaAi.Modules.Catalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Catalogs")]
[Trait("Layer", "Application")]
public class DeleteServiceCategoryCommandHandlerTests
{
    private readonly Mock<IServiceCategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly DeleteServiceCategoryCommandHandler _handler;

    public DeleteServiceCategoryCommandHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IServiceCategoryRepository>();
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _handler = new DeleteServiceCategoryCommandHandler(_categoryRepositoryMock.Object, _serviceRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().WithName("Limpeza").Build();
        var command = new DeleteServiceCategoryCommand(category.Id.Value);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _serviceRepositoryMock
            .Setup(x => x.CountByCategoryAsync(It.IsAny<ServiceCategoryId>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _categoryRepositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _categoryRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldReturnFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new DeleteServiceCategoryCommand(categoryId);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceCategory?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("not found");
        _categoryRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithAssociatedServices_ShouldReturnFailure()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().WithName("Limpeza").Build();
        var command = new DeleteServiceCategoryCommand(category.Id.Value);

        _categoryRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _serviceRepositoryMock
            .Setup(x => x.CountByCategoryAsync(It.IsAny<ServiceCategoryId>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Cannot delete");
        _categoryRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
