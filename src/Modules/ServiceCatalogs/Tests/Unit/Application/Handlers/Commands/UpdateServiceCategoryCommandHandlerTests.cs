using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Database;

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
        _handler = new UpdateServiceCategoryCommandHandler(_uowMock.Object, _queriesMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().WithName("Original Name").Build();
        var command = new UpdateServiceCategoryCommand(category.Id.Value, "Updated Name", "Updated Description", 2);

        _repositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _queriesMock
            .Setup(x => x.ExistsWithNameAsync(command.Name, It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.Name.Should().Be(command.Name);
        category.Description.Should().Be(command.Description);
        category.DisplayOrder.Should().Be(command.DisplayOrder);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldReturnFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new UpdateServiceCategoryCommand(categoryId, "Updated Name", "Updated Description", 2);

        _repositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceCategory?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be(ValidationMessages.NotFound.Category);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _queriesMock
            .Setup(x => x.ExistsWithNameAsync(command.Name, It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be(string.Format(ValidationMessages.Catalogs.CategoryNameExists, command.Name));
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
        result.Error!.Message.Should().Be(ValidationMessages.Required.Id);
        _repositoryMock.Verify(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()), Times.Never);
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
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be(ValidationMessages.Required.CategoryName);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDomainExceptionThrown_ShouldReturnFailureWithMessage()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().Build();
        var command = new UpdateServiceCategoryCommand(category.Id.Value, "Valid Name", "Description", 1);

        _repositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _queriesMock
            .Setup(x => x.ExistsWithNameAsync(It.IsAny<string>(), It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CatalogDomainException("Domain rule violation"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Domain rule violation");
    }
}
