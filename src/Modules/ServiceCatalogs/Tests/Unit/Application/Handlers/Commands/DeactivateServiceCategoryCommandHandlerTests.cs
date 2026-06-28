using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.ServiceCatalogs;


namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class DeactivateServiceCategoryCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<ServiceCategory, ServiceCategoryId>> _repositoryMock;
    private readonly DeactivateServiceCategoryCommandHandler _handler;

    public DeactivateServiceCategoryCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<ServiceCategory, ServiceCategoryId>>();
        
        _uowMock.Setup(x => x.GetRepository<ServiceCategory, ServiceCategoryId>()).Returns(_repositoryMock.Object);
        _handler = new DeactivateServiceCategoryCommandHandler(_uowMock.Object, NullLogger<DeactivateServiceCategoryCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var category = new ServiceCategoryBuilder()
            .WithName("Limpeza")
            .AsActive()
            .Build();
        var command = new DeactivateServiceCategoryCommand(category.Id.Value);

        _repositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.IsActive.Should().BeFalse();
        _repositoryMock.Verify(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldReturnFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new DeactivateServiceCategoryCommand(categoryId);

        _repositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceCategory?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be(string.Format(ValidationMessages.NotFound.CategoryById, categoryId));
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyId_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeactivateServiceCategoryCommand(Guid.Empty);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be(ValidationMessages.Required.Id);
        _repositoryMock.Verify(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithAlreadyInactiveCategory_ShouldReturnSuccess()
    {
        // Arrange
        var category = new ServiceCategoryBuilder()
            .WithName("Limpeza")
            .AsInactive()
            .Build();
        var command = new DeactivateServiceCategoryCommand(category.Id.Value);

        _repositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.IsActive.Should().BeFalse();
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesThrows_ShouldReturnGenericFailure()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var command = new DeactivateServiceCategoryCommand(category.Id.Value);

        _repositoryMock
            .Setup(x => x.TryFindAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failure"));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Ocorreu um erro inesperado ao desativar a categoria de serviço.");
    }
}