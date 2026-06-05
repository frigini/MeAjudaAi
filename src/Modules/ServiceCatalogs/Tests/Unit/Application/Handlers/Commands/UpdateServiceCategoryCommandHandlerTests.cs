using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class UpdateServiceCategoryCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<ServiceCategory, ServiceCategoryId>> _repositoryMock;
    private readonly Mock<IServiceCategoryQueries> _queriesMock;
    private readonly UpdateServiceCategoryCommandHandler _handler;

    public UpdateServiceCategoryCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<ServiceCategory, ServiceCategoryId>>();
        _queriesMock = new Mock<IServiceCategoryQueries>();
        
        _uowMock.Setup(x => x.GetRepository<ServiceCategory, ServiceCategoryId>()).Returns(_repositoryMock.Object);
        _handler = new UpdateServiceCategoryCommandHandler(_uowMock.Object, _queriesMock.Object, NullLogger<UpdateServiceCategoryCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().WithName("Original Name").Build();
        var command = new UpdateServiceCategoryCommand(category.Id.Value, "Updated Name", "Updated Description", 2);

        _repositoryMock
            .Setup(x => x.TryFindAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _queriesMock
            .Setup(x => x.ExistsWithNameAsync(command.Name, category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.Name.Should().Be("Updated Name");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldReturnFailure()
    {
        var categoryId = ServiceCategoryId.From(Guid.NewGuid());
        _repositoryMock
            .Setup(x => x.TryFindAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceCategory?)null);
        var command = new UpdateServiceCategoryCommand(categoryId.Value, "Name", "Desc", 1);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("não encontrada");
        _repositoryMock.Verify(x => x.TryFindAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnFailure()
    {
        var category = new ServiceCategoryBuilder().WithName("Original").Build();
        _repositoryMock
            .Setup(x => x.TryFindAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _queriesMock
            .Setup(x => x.ExistsWithNameAsync("Duplicate", category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var command = new UpdateServiceCategoryCommand(category.Id.Value, "Duplicate", "Desc", 1);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Duplicate");
        _queriesMock.Verify(x => x.ExistsWithNameAsync("Duplicate", category.Id, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyId_ShouldReturnFailure()
    {
        var command = new UpdateServiceCategoryCommand(Guid.Empty, "Name", "Desc", 1);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Be(ValidationMessages.Required.Id);
        _repositoryMock.Verify(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyName_ShouldReturnFailure()
    {
        var category = new ServiceCategoryBuilder().Build();
        _repositoryMock
            .Setup(x => x.TryFindAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        var command = new UpdateServiceCategoryCommand(category.Id.Value, string.Empty, "Desc", 1);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("nome");
        _repositoryMock.Verify(x => x.TryFindAsync(category.Id, It.IsAny<CancellationToken>()), Times.Once);
        _queriesMock.Verify(x => x.ExistsWithNameAsync(It.IsAny<string>(), It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesThrows_ShouldReturnGenericFailure()
    {
        var category = new ServiceCategoryBuilder().WithName("Original").Build();
        _repositoryMock
            .Setup(x => x.TryFindAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _queriesMock
            .Setup(x => x.ExistsWithNameAsync("New Name", category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failure"));

        var command = new UpdateServiceCategoryCommand(category.Id.Value, "New Name", "Desc", 1);
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Ocorreu um erro inesperado ao atualizar a categoria de serviço.");
    }
}

// Minimal Builder for test
internal class ServiceCategoryBuilder
{
    private ServiceCategory _category = ServiceCategory.Create("Default", "Desc", 1);
    public ServiceCategoryBuilder WithName(string name) { _category.Update(name, _category.Description, _category.DisplayOrder); return this; }
    public ServiceCategoryBuilder AsActive() { _category.Activate(); return this; }
    public ServiceCategoryBuilder AsInactive() { _category.Deactivate(); return this; }
    public ServiceCategory Build() => _category;
}



