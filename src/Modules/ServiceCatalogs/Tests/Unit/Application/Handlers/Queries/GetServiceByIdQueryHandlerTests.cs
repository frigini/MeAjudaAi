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
public class GetServiceByIdQueryHandlerTests
{
    private readonly Mock<IServiceQueries> _serviceQueriesMock;
    private readonly GetServiceByIdQueryHandler _handler;

    public GetServiceByIdQueryHandlerTests()
    {
        _serviceQueriesMock = new Mock<IServiceQueries>();
        _handler = new GetServiceByIdQueryHandler(_serviceQueriesMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingService_ShouldReturnSuccess()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Serviço Teste")
            .Build();
        var query = new GetServiceByIdQuery(service.Id.Value);

        _serviceQueriesMock.Setup(x => x.GetByIdAsync(service.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        var result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value?.Name.Should().Be("Serviço Teste");
    }

    [Fact]
    public async Task Handle_WithNonExistingService_ShouldReturnNull()
    {
        var query = new GetServiceByIdQuery(Guid.NewGuid());

        _serviceQueriesMock.Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        var result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithEmptyId_ShouldReturnFailure()
    {
        var query = new GetServiceByIdQuery(Guid.Empty);

        var result = await _handler.HandleAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}