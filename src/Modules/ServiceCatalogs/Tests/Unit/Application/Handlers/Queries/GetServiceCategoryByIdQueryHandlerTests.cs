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
public class GetServiceCategoryByIdQueryHandlerTests
{
    private readonly Mock<IServiceCategoryQueries> _categoryQueriesMock;
    private readonly GetServiceCategoryByIdQueryHandler _handler;

    public GetServiceCategoryByIdQueryHandlerTests()
    {
        _categoryQueriesMock = new Mock<IServiceCategoryQueries>();
        _handler = new GetServiceCategoryByIdQueryHandler(_categoryQueriesMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingCategory_ShouldReturnSuccess()
    {
        var category = new ServiceCategoryBuilder().WithName("Teste").Build();
        var query = new GetServiceCategoryByIdQuery(category.Id.Value);

        _categoryQueriesMock.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value?.Name.Should().Be("Teste");
    }

    [Fact]
    public async Task Handle_WithNonExistingCategory_ShouldReturnNull()
    {
        var query = new GetServiceCategoryByIdQuery(Guid.NewGuid());

        _categoryQueriesMock.Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceCategory?)null);

        var result = await _handler.HandleAsync(query, CancellationToken.None);

        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithEmptyId_ShouldReturnFailure()
    {
        var query = new GetServiceCategoryByIdQuery(Guid.Empty);

        var result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}