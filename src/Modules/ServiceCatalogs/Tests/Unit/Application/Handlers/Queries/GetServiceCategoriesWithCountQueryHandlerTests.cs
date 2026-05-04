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
    public async Task Handle_ShouldReturnCategories()
    {
        var category = new ServiceCategoryBuilder().WithName("Cat 1").Build();
        var categories = new List<ServiceCategory> { category };
        var services = new List<Service> { new ServiceBuilder().WithCategoryId(category.Id).Build() };
        var query = new GetServiceCategoriesWithCountQuery(ActiveOnly: false);

        _categoryQueriesMock.Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);
        _serviceQueriesMock.Setup(x => x.GetAllAsync(false, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);
        _serviceQueriesMock.Setup(x => x.GetAllAsync(true, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Service>());

        var result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}