using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using Moq;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Queries;
[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class GetServicesByCategoryQueryHandlerTests
{
    private readonly Mock<IServiceQueries> _serviceQueriesMock;
    private readonly GetServicesByCategoryQueryHandler _handler;

    public GetServicesByCategoryQueryHandlerTests()
    {
        _serviceQueriesMock = new Mock<IServiceQueries>();
        _handler = new GetServicesByCategoryQueryHandler(_serviceQueriesMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnServices()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var services = new List<Service>
        {
            new ServiceBuilder().WithCategoryId(category.Id).WithName("Serviço 1").Build()
        };
        var query = new GetServicesByCategoryQuery(category.Id.Value, ActiveOnly: false);

        _serviceQueriesMock.Setup(x => x.GetByCategoryAsync(category.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        var result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}