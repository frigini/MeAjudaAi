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
public class GetAllServicesQueryHandlerTests
{
    private readonly Mock<IServiceQueries> _serviceQueriesMock;
    private readonly GetAllServicesQueryHandler _handler;

    public GetAllServicesQueryHandlerTests()
    {
        _serviceQueriesMock = new Mock<IServiceQueries>();
        _handler = new GetAllServicesQueryHandler(_serviceQueriesMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnServices()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var services = new List<Service>
        {
            new ServiceBuilder().WithCategoryId(category.Id).WithName("Serviço 1").Build(),
            new ServiceBuilder().WithCategoryId(category.Id).WithName("Serviço 2").Build()
        };
        var query = new GetAllServicesQuery(ActiveOnly: false, Name: null);

        _serviceQueriesMock.Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        var result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithActiveOnly_ShouldReturnActiveServices()
    {
        var query = new GetAllServicesQuery(ActiveOnly: true);

        _serviceQueriesMock.Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Service>());

        var result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}