using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.ServiceCatalogs;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class GetServiceCategoriesWithCountQueryHandlerTests
{
    private readonly Mock<IServiceCategoryQueries> _categoryQueriesMock;
    private readonly Mock<IServiceQueries> _serviceQueriesMock;
    private readonly GetServiceCategoriesWithCountQueryHandler _handler;

    public GetServiceCategoriesWithCountQueryHandlerTests()
    {
        _categoryQueriesMock = new Mock<IServiceCategoryQueries>();
        _serviceQueriesMock = new Mock<IServiceQueries>();
        _handler = new GetServiceCategoriesWithCountQueryHandler(_categoryQueriesMock.Object, _serviceQueriesMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCategoriesWithCounts()
    {
        // Arrange
        var query = new GetServiceCategoriesWithCountQuery(ActiveOnly: false);
        var category1 = new ServiceCategoryBuilder().WithName("Limpeza").AsActive().Build();
        var category2 = new ServiceCategoryBuilder().WithName("Reparos").AsActive().Build();
        var categories = new List<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory>
        {
            category1,
            category2
        };

        _categoryQueriesMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var counts = new Dictionary<ServiceCategoryId, (int Total, int Active)>
        {
            [category1.Id] = (5, 3),
            [category2.Id] = (8, 6)
        };

        _serviceQueriesMock
            .Setup(x => x.CountByCategoriesAsync(It.IsAny<IEnumerable<ServiceCategoryId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(counts);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        var limpeza = result.Value.First(c => c.Name == "Limpeza");
        limpeza.TotalServicesCount.Should().Be(5);
        limpeza.ActiveServicesCount.Should().Be(3);

        var reparos = result.Value.First(c => c.Name == "Reparos");
        reparos.TotalServicesCount.Should().Be(8);
        reparos.ActiveServicesCount.Should().Be(6);

        _categoryQueriesMock.Verify(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()), Times.Once);
        _serviceQueriesMock.Verify(x => x.CountByCategoriesAsync(It.IsAny<IEnumerable<ServiceCategoryId>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithActiveOnlyTrue_ShouldReturnOnlyActiveCategories()
    {
        // Arrange
        var query = new GetServiceCategoriesWithCountQuery(ActiveOnly: true);
        var category1 = new ServiceCategoryBuilder().WithName("Limpeza").AsActive().Build();
        var category2 = new ServiceCategoryBuilder().WithName("Reparos").AsActive().Build();
        var categories = new List<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory>
        {
            category1,
            category2
        };

        _categoryQueriesMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var counts = new Dictionary<ServiceCategoryId, (int Total, int Active)>
        {
            [category1.Id] = (5, 3),
            [category2.Id] = (8, 6)
        };

        _serviceQueriesMock
            .Setup(x => x.CountByCategoriesAsync(It.IsAny<IEnumerable<ServiceCategoryId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(counts);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(c => c.IsActive);

        _categoryQueriesMock.Verify(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoCategories_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetServiceCategoriesWithCountQuery(ActiveOnly: false);

        _categoryQueriesMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory>());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _categoryQueriesMock.Verify(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()), Times.Once);
        _serviceQueriesMock.Verify(x => x.CountByCategoriesAsync(It.IsAny<IEnumerable<ServiceCategoryId>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithCategoriesWithNoServices_ShouldReturnZeroCounts()
    {
        // Arrange
        var query = new GetServiceCategoriesWithCountQuery(ActiveOnly: false);
        var category = new ServiceCategoryBuilder().WithName("Limpeza").AsActive().Build();
        var categories = new List<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory> { category };

        _categoryQueriesMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        _serviceQueriesMock
            .Setup(x => x.CountByCategoriesAsync(It.IsAny<IEnumerable<ServiceCategoryId>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<ServiceCategoryId, (int Total, int Active)>());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().TotalServicesCount.Should().Be(0);
        result.Value.First().ActiveServicesCount.Should().Be(0);
    }
}
