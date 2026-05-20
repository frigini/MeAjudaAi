using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class GetServiceByIdQueryHandlerTests
{
    private readonly Mock<IServiceQueries> _queriesMock;
    private readonly GetServiceByIdQueryHandler _handler;

    public GetServiceByIdQueryHandlerTests()
    {
        _queriesMock = new Mock<IServiceQueries>();
        _handler = new GetServiceByIdQueryHandler(_queriesMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingService_ShouldReturnSuccess()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var service = new ServiceBuilder()
            .WithCategoryId(categoryId)
            .WithName("Limpeza de Piscina")
            .WithDescription("Limpeza profunda de piscina")
            .Build();
        var query = new GetServiceByIdQuery(service.Id.Value);

        _queriesMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(service.Id.Value);
        result.Value.CategoryId.Should().Be(categoryId);
        result.Value.Name.Should().Be("Limpeza de Piscina");
        result.Value.Description.Should().Be("Limpeza profunda de piscina");

        _queriesMock.Verify(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ShouldReturnNull()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var query = new GetServiceByIdQuery(serviceId);

        _queriesMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();

        _queriesMock.Verify(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
