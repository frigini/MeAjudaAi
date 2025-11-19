using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class CreateServiceCategoryCommandHandlerTests
{
    private readonly Mock<IServiceCategoryRepository> _repositoryMock;
    private readonly CreateServiceCategoryCommandHandler _handler;

    public CreateServiceCategoryCommandHandlerTests()
    {
        _repositoryMock = new Mock<IServiceCategoryRepository>();
        _handler = new CreateServiceCategoryCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new CreateServiceCategoryCommand("Limpeza", "Serviços de limpeza", 1);

        _repositoryMock
            .Setup(x => x.ExistsWithNameAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ServiceCategory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBe(Guid.Empty);

        _repositoryMock.Verify(x => x.ExistsWithNameAsync(command.Name, null, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<ServiceCategory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateServiceCategoryCommand("Limpeza", "Serviços de limpeza", 1);

        _repositoryMock
            .Setup(x => x.ExistsWithNameAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("already exists");

        _repositoryMock.Verify(x => x.ExistsWithNameAsync(command.Name, null, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<ServiceCategory>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Handle_WithInvalidName_ShouldReturnFailure(string? invalidName)
    {
        // Arrange
        var command = new CreateServiceCategoryCommand(invalidName!, "Description", 1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("name", "validation error should mention the problematic field");
    }
}
