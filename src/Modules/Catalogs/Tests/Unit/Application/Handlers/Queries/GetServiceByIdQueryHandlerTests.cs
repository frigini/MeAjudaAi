using MeAjudaAi.Modules.Catalogs.Application.Handlers.Queries.Service;
using MeAjudaAi.Modules.Catalogs.Application.Queries.Service;
using MeAjudaAi.Modules.Catalogs.Domain.Entities;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.Catalogs.Tests.Builders;

namespace MeAjudaAi.Modules.Catalogs.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Catalogs")]
[Trait("Layer", "Application")]
public class GetServiceByIdQueryHandlerTests
{
    private readonly Mock<IServiceRepository> _repositoryMock;
    private readonly GetServiceByIdQueryHandler _handler;

    public GetServiceByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IServiceRepository>();
        _handler = new GetServiceByIdQueryHandler(_repositoryMock.Object);
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

        _repositoryMock
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

        _repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ShouldReturnNull()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var query = new GetServiceByIdQuery(serviceId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();

        _repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
