using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class GetServicesByCategoryQueryHandlerTests
{
    private readonly Mock<IServiceQueries> _queriesMock;
    private readonly GetServicesByCategoryQueryHandler _handler;

    public GetServicesByCategoryQueryHandlerTests()
    {
        _queriesMock = new Mock<IServiceQueries>();
        _handler = new GetServicesByCategoryQueryHandler(_queriesMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingCategory_ShouldReturnServices()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var serviceCategoryId = ServiceCategoryId.From(categoryId);
        var query = new GetServicesByCategoryQuery(categoryId, ActiveOnly: false);
        var services = new List<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service>
        {
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Service 1").Build(),
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Service 2").Build()
        };

        _queriesMock
            .Setup(x => x.GetByCategoryAsync(serviceCategoryId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(s => s.CategoryId.Should().Be(categoryId));

        _queriesMock.Verify(x => x.GetByCategoryAsync(serviceCategoryId, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithActiveOnlyTrue_ShouldReturnOnlyActiveServices()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var serviceCategoryId = ServiceCategoryId.From(categoryId);
        var query = new GetServicesByCategoryQuery(categoryId, ActiveOnly: true);
        var services = new List<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service>
        {
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Active").AsActive().Build()
        };

        _queriesMock
            .Setup(x => x.GetByCategoryAsync(serviceCategoryId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.Should().OnlyContain(s => s.IsActive);

        _queriesMock.Verify(x => x.GetByCategoryAsync(serviceCategoryId, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoServices_ShouldReturnEmptyList()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var serviceCategoryId = ServiceCategoryId.From(categoryId);
        var query = new GetServicesByCategoryQuery(categoryId, ActiveOnly: false);

        _queriesMock
            .Setup(x => x.GetByCategoryAsync(serviceCategoryId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service>());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _queriesMock.Verify(x => x.GetByCategoryAsync(serviceCategoryId, false, It.IsAny<CancellationToken>()), Times.Once);
    }
}
