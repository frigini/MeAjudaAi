using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.ServiceCatalogs;


namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class DeleteServiceCategoryCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<ServiceCategory, ServiceCategoryId>> _categoryRepositoryMock;
    private readonly Mock<IServiceQueries> _serviceQueriesMock;
    private readonly DeleteServiceCategoryCommandHandler _handler;

    public DeleteServiceCategoryCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _categoryRepositoryMock = new Mock<IRepository<ServiceCategory, ServiceCategoryId>>();
        _serviceQueriesMock = new Mock<IServiceQueries>();
        
        _uowMock.Setup(x => x.GetRepository<ServiceCategory, ServiceCategoryId>()).Returns(_categoryRepositoryMock.Object);
        _handler = new DeleteServiceCategoryCommandHandler(_uowMock.Object, _serviceQueriesMock.Object, NullLogger<DeleteServiceCategoryCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().WithName("Limpeza").Build();
        var command = new DeleteServiceCategoryCommand(category.Id.Value);

        _categoryRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _serviceQueriesMock
            .Setup(x => x.CountByCategoryAsync(It.IsAny<ServiceCategoryId>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _categoryRepositoryMock.Verify(x => x.Delete(It.IsAny<ServiceCategory>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldReturnFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new DeleteServiceCategoryCommand(categoryId);

        _categoryRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceCategory?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be(ValidationMessages.NotFound.Category);
        _categoryRepositoryMock.Verify(x => x.Delete(It.IsAny<ServiceCategory>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithAssociatedServices_ShouldReturnFailure()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().WithName("Limpeza").Build();
        var command = new DeleteServiceCategoryCommand(category.Id.Value);

        _categoryRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _serviceQueriesMock
            .Setup(x => x.CountByCategoryAsync(It.IsAny<ServiceCategoryId>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be(string.Format(ValidationMessages.Catalogs.CannotDeleteCategoryWithServices, 3));
        _categoryRepositoryMock.Verify(x => x.Delete(It.IsAny<ServiceCategory>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyId_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeleteServiceCategoryCommand(Guid.Empty);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be(ValidationMessages.Required.Id);
        _categoryRepositoryMock.Verify(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesThrows_ShouldReturnGenericFailure()
    {
        var category = new ServiceCategoryBuilder().WithName("Limpeza").Build();
        var command = new DeleteServiceCategoryCommand(category.Id.Value);

        _categoryRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _serviceQueriesMock
            .Setup(x => x.CountByCategoryAsync(It.IsAny<ServiceCategoryId>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failure"));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Ocorreu um erro inesperado ao excluir a categoria de serviço.");
    }
}