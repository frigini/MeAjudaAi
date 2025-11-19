using MeAjudaAi.Modules.Catalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.Catalogs.Application.Handlers.Commands;
using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.Catalogs.Tests.Builders;

namespace MeAjudaAi.Modules.Catalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Catalogs")]
[Trait("Layer", "Application")]
public class UpdateServiceCategoryCommandHandlerTests
{
    private readonly Mock<IServiceCategoryRepository> _repositoryMock;
    private readonly UpdateServiceCategoryCommandHandler _handler;

    public UpdateServiceCategoryCommandHandlerTests()
    {
        _repositoryMock = new Mock<IServiceCategoryRepository>();
        _handler = new UpdateServiceCategoryCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var category = new ServiceCategoryBuilder()
            .WithName("Original Name")
            .Build();
        var command = new UpdateServiceCategoryCommand(category.Id.Value, "Updated Name", "Updated Description", 2);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _repositoryMock
            .Setup(x => x.ExistsWithNameAsync(command.Name, It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<ServiceCategory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ServiceCategory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldReturnFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new UpdateServiceCategoryCommand(categoryId, "Updated Name", "Updated Description", 2);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceCategory?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("not found");
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ServiceCategory>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnFailure()
    {
        // Arrange
        var category = new ServiceCategoryBuilder()
            .WithName("Original Name")
            .Build();
        var command = new UpdateServiceCategoryCommand(category.Id.Value, "Duplicate Name", "Description", 2);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _repositoryMock
            .Setup(x => x.ExistsWithNameAsync(command.Name, It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("already exists");
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ServiceCategory>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
