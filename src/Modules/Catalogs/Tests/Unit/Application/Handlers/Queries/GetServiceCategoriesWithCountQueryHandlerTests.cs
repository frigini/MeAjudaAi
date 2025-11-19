using MeAjudaAi.Modules.Catalogs.Application.Handlers.Queries.ServiceCategory;
using MeAjudaAi.Modules.Catalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Tests.Builders;

namespace MeAjudaAi.Modules.Catalogs.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Catalogs")]
[Trait("Layer", "Application")]
public class GetServiceCategoriesWithCountQueryHandlerTests
{
    private readonly Mock<IServiceCategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly GetServiceCategoriesWithCountQueryHandler _handler;

    public GetServiceCategoriesWithCountQueryHandlerTests()
    {
        _categoryRepositoryMock = new Mock<IServiceCategoryRepository>();
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _handler = new GetServiceCategoriesWithCountQueryHandler(_categoryRepositoryMock.Object, _serviceRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCategoriesWithCounts()
    {
        // Arrange
        var query = new GetServiceCategoriesWithCountQuery(ActiveOnly: false);
        var category1 = new ServiceCategoryBuilder().WithName("Limpeza").AsActive().Build();
        var category2 = new ServiceCategoryBuilder().WithName("Reparos").AsActive().Build();
        var categories = new List<MeAjudaAi.Modules.Catalogs.Domain.Entities.ServiceCategory>
        {
            category1,
            category2
        };

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        _serviceRepositoryMock
            .Setup(x => x.CountByCategoryAsync(category1.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        _serviceRepositoryMock
            .Setup(x => x.CountByCategoryAsync(category1.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        _serviceRepositoryMock
            .Setup(x => x.CountByCategoryAsync(category2.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(8);

        _serviceRepositoryMock
            .Setup(x => x.CountByCategoryAsync(category2.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(6);

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

        _categoryRepositoryMock.Verify(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(x => x.CountByCategoryAsync(It.IsAny<MeAjudaAi.Modules.Catalogs.Domain.ValueObjects.ServiceCategoryId>(), false, It.IsAny<CancellationToken>()), Times.Exactly(2));
        _serviceRepositoryMock.Verify(x => x.CountByCategoryAsync(It.IsAny<MeAjudaAi.Modules.Catalogs.Domain.ValueObjects.ServiceCategoryId>(), true, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WithActiveOnlyTrue_ShouldReturnOnlyActiveCategories()
    {
        // Arrange
        var query = new GetServiceCategoriesWithCountQuery(ActiveOnly: true);
        var category1 = new ServiceCategoryBuilder().WithName("Limpeza").AsActive().Build();
        var category2 = new ServiceCategoryBuilder().WithName("Reparos").AsActive().Build();
        var categories = new List<MeAjudaAi.Modules.Catalogs.Domain.Entities.ServiceCategory>
        {
            category1,
            category2
        };

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        _serviceRepositoryMock
            .Setup(x => x.CountByCategoryAsync(It.IsAny<MeAjudaAi.Modules.Catalogs.Domain.ValueObjects.ServiceCategoryId>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        _serviceRepositoryMock
            .Setup(x => x.CountByCategoryAsync(It.IsAny<MeAjudaAi.Modules.Catalogs.Domain.ValueObjects.ServiceCategoryId>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(c => c.IsActive);

        _categoryRepositoryMock.Verify(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoCategories_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetServiceCategoriesWithCountQuery(ActiveOnly: false);

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MeAjudaAi.Modules.Catalogs.Domain.Entities.ServiceCategory>());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _categoryRepositoryMock.Verify(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(x => x.CountByCategoryAsync(It.IsAny<MeAjudaAi.Modules.Catalogs.Domain.ValueObjects.ServiceCategoryId>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithCategoriesWithNoServices_ShouldReturnZeroCounts()
    {
        // Arrange
        var query = new GetServiceCategoriesWithCountQuery(ActiveOnly: false);
        var category = new ServiceCategoryBuilder().WithName("Limpeza").AsActive().Build();
        var categories = new List<MeAjudaAi.Modules.Catalogs.Domain.Entities.ServiceCategory> { category };

        _categoryRepositoryMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        _serviceRepositoryMock
            .Setup(x => x.CountByCategoryAsync(category.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _serviceRepositoryMock
            .Setup(x => x.CountByCategoryAsync(category.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().TotalServicesCount.Should().Be(0);
        result.Value.First().ActiveServicesCount.Should().Be(0);
    }
}
