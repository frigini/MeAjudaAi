using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
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

    [Fact]
    public async Task Handle_WithEmptyCategoryId_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateServiceCategoryCommand(Guid.Empty, "Name", "Description", 1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("cannot be empty");
        _repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithEmptyName_ShouldReturnFailure(string? emptyName)
    {
        // Arrange
        var category = new ServiceCategoryBuilder().Build();
        var command = new UpdateServiceCategoryCommand(category.Id.Value, emptyName!, "Description", 1);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("cannot be empty");
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ServiceCategory>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDomainExceptionThrown_ShouldReturnFailureWithMessage()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().Build();
        var command = new UpdateServiceCategoryCommand(category.Id.Value, "Valid Name", "Description", 1);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _repositoryMock
            .Setup(x => x.ExistsWithNameAsync(It.IsAny<string>(), It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<ServiceCategory>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CatalogDomainException("Domain rule violation"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Domain rule violation");
    }
}
