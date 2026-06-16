using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.ServiceCatalogs;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class GetServiceCategoryByIdQueryHandlerTests
{
    private readonly Mock<IServiceCategoryQueries> _queriesMock;
    private readonly GetServiceCategoryByIdQueryHandler _handler;

    public GetServiceCategoryByIdQueryHandlerTests()
    {
        _queriesMock = new Mock<IServiceCategoryQueries>();
        _handler = new GetServiceCategoryByIdQueryHandler(_queriesMock.Object);
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

        _queriesMock
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

        _queriesMock.Verify(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldReturnNull()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetServiceCategoryByIdQuery(categoryId);

        _queriesMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();

        _queriesMock.Verify(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
