using MeAjudaAi.Modules.Catalogs.Application.Handlers.Queries.Service;
using MeAjudaAi.Modules.Catalogs.Application.Queries.Service;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.Catalogs.Tests.Builders;

namespace MeAjudaAi.Modules.Catalogs.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Catalogs")]
[Trait("Layer", "Application")]
public class GetServicesByCategoryQueryHandlerTests
{
    private readonly Mock<IServiceRepository> _repositoryMock;
    private readonly GetServicesByCategoryQueryHandler _handler;

    public GetServicesByCategoryQueryHandlerTests()
    {
        _repositoryMock = new Mock<IServiceRepository>();
        _handler = new GetServicesByCategoryQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingCategory_ShouldReturnServices()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetServicesByCategoryQuery(categoryId, ActiveOnly: false);
        var services = new List<MeAjudaAi.Modules.Catalogs.Domain.Entities.Service>
        {
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Service 1").Build(),
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Service 2").Build()
        };

        _repositoryMock
            .Setup(x => x.GetByCategoryAsync(It.IsAny<ServiceCategoryId>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(s => s.CategoryId.Should().Be(categoryId));

        _repositoryMock.Verify(x => x.GetByCategoryAsync(It.IsAny<ServiceCategoryId>(), false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithActiveOnlyTrue_ShouldReturnOnlyActiveServices()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetServicesByCategoryQuery(categoryId, ActiveOnly: true);
        var services = new List<MeAjudaAi.Modules.Catalogs.Domain.Entities.Service>
        {
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Active").AsActive().Build()
        };

        _repositoryMock
            .Setup(x => x.GetByCategoryAsync(It.IsAny<ServiceCategoryId>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.Should().OnlyContain(s => s.IsActive);

        _repositoryMock.Verify(x => x.GetByCategoryAsync(It.IsAny<ServiceCategoryId>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoServices_ShouldReturnEmptyList()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetServicesByCategoryQuery(categoryId, ActiveOnly: false);

        _repositoryMock
            .Setup(x => x.GetByCategoryAsync(It.IsAny<ServiceCategoryId>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MeAjudaAi.Modules.Catalogs.Domain.Entities.Service>());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _repositoryMock.Verify(x => x.GetByCategoryAsync(It.IsAny<ServiceCategoryId>(), false, It.IsAny<CancellationToken>()), Times.Once);
    }
}
