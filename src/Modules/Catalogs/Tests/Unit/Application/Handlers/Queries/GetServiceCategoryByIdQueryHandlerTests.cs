using MeAjudaAi.Modules.Catalogs.Application.Handlers.Queries.ServiceCategory;
using MeAjudaAi.Modules.Catalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.Catalogs.Tests.Builders;

namespace MeAjudaAi.Modules.Catalogs.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Catalogs")]
[Trait("Layer", "Application")]
public class GetServiceCategoryByIdQueryHandlerTests
{
    private readonly Mock<IServiceCategoryRepository> _repositoryMock;
    private readonly GetServiceCategoryByIdQueryHandler _handler;

    public GetServiceCategoryByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IServiceCategoryRepository>();
        _handler = new GetServiceCategoryByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingCategory_ShouldReturnSuccess()
    {
        // Arrange
        var category = new ServiceCategoryBuilder()
            .WithName("Limpeza")
            .WithDescription("Serviços de limpeza")
            .Build();
        var query = new GetServiceCategoryByIdQuery(category.Id.Value);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(category.Id.Value);
        result.Value.Name.Should().Be("Limpeza");
        result.Value.Description.Should().Be("Serviços de limpeza");

        _repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldReturnNull()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetServiceCategoryByIdQuery(categoryId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MeAjudaAi.Modules.Catalogs.Domain.Entities.ServiceCategory?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();

        _repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
