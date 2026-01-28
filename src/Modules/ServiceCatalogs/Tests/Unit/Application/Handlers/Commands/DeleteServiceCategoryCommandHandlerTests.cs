using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
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
// ...
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
        result.Error!.Message.Should().Be(ValidationMessages.NotFound.Category);
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
        result.Error!.Message.Should().Be(string.Format(ValidationMessages.Catalogs.CannotDeleteCategoryWithServices, 3));
        _categoryRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
