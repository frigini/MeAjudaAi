using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using Moq;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Queries;
[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class GetAllServiceCategoriesQueryHandlerTests
{
    private readonly Mock<IServiceCategoryQueries> _categoryQueriesMock;
    private readonly GetAllServiceCategoriesQueryHandler _handler;

    public GetAllServiceCategoriesQueryHandlerTests()
    {
        _categoryQueriesMock = new Mock<IServiceCategoryQueries>();
        _handler = new GetAllServiceCategoriesQueryHandler(_categoryQueriesMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnCategories()
    {
        var categories = new List<ServiceCategory>
        {
            new ServiceCategoryBuilder().WithName("Cat 1").Build(),
            new ServiceCategoryBuilder().WithName("Cat 2").Build()
        };
        var query = new GetAllServiceCategoriesQuery(ActiveOnly: false);

        _categoryQueriesMock.Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithActiveOnly_ShouldReturnActiveCategories()
    {
        var query = new GetAllServiceCategoriesQuery(ActiveOnly: true);

        _categoryQueriesMock.Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ServiceCategory>());

        var result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
