using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.ServiceCatalogs;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class GetAllServiceCategoriesQueryHandlerTests
{
    private readonly Mock<IServiceCategoryQueries> _queriesMock;
    private readonly GetAllServiceCategoriesQueryHandler _handler;

    public GetAllServiceCategoriesQueryHandlerTests()
    {
        _queriesMock = new Mock<IServiceCategoryQueries>();
        _handler = new GetAllServiceCategoriesQueryHandler(_queriesMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllCategories()
    {
        // Arrange
        var query = new GetAllServiceCategoriesQuery(ActiveOnly: false);
        var categories = new List<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory>
        {
            new ServiceCategoryBuilder().WithName("Limpeza").Build(),
            new ServiceCategoryBuilder().WithName("Reparos").Build(),
            new ServiceCategoryBuilder().WithName("Pintura").Build()
        };

        _queriesMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value.Should().Contain(c => c.Name == "Limpeza");
        result.Value.Should().Contain(c => c.Name == "Reparos");
        result.Value.Should().Contain(c => c.Name == "Pintura");

        _queriesMock.Verify(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithActiveOnlyTrue_ShouldReturnOnlyActiveCategories()
    {
        // Arrange
        var query = new GetAllServiceCategoriesQuery(ActiveOnly: true);
        var categories = new List<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory>
        {
            new ServiceCategoryBuilder().WithName("Limpeza").AsActive().Build(),
            new ServiceCategoryBuilder().WithName("Reparos").AsActive().Build()
        };

        _queriesMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(c => c.IsActive);

        _queriesMock.Verify(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoCategories_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllServiceCategoriesQuery(ActiveOnly: false);

        _queriesMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory>());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _queriesMock.Verify(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }
}
