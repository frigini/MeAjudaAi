using MeAjudaAi.Modules.Catalogs.Application.Handlers.Queries;
using MeAjudaAi.Modules.Catalogs.Application.Queries;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Tests.Builders;

namespace MeAjudaAi.Modules.Catalogs.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Catalogs")]
[Trait("Layer", "Application")]
public class GetAllServicesQueryHandlerTests
{
    private readonly Mock<IServiceRepository> _repositoryMock;
    private readonly GetAllServicesQueryHandler _handler;

    public GetAllServicesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IServiceRepository>();
        _handler = new GetAllServicesQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllServices()
    {
        // Arrange
        var query = new GetAllServicesQuery(ActiveOnly: false);
        var categoryId = Guid.NewGuid();
        var services = new List<MeAjudaAi.Modules.Catalogs.Domain.Entities.Service>
        {
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Service 1").Build(),
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Service 2").Build(),
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Service 3").Build()
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);

        _repositoryMock.Verify(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithActiveOnlyTrue_ShouldReturnOnlyActiveServices()
    {
        // Arrange
        var query = new GetAllServicesQuery(ActiveOnly: true);
        var categoryId = Guid.NewGuid();
        var services = new List<MeAjudaAi.Modules.Catalogs.Domain.Entities.Service>
        {
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Active 1").AsActive().Build(),
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Active 2").AsActive().Build()
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(s => s.IsActive);

        _repositoryMock.Verify(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoServices_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllServicesQuery(ActiveOnly: false);

        _repositoryMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MeAjudaAi.Modules.Catalogs.Domain.Entities.Service>());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _repositoryMock.Verify(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }
}
