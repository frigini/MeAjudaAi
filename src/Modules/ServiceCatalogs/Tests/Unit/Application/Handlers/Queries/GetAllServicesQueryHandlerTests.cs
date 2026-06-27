using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Queries.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.ServiceCatalogs;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class GetAllServicesQueryHandlerTests
{
    private readonly Mock<IServiceQueries> _queriesMock;
    private readonly GetAllServicesQueryHandler _handler;

    public GetAllServicesQueryHandlerTests()
    {
        _queriesMock = new Mock<IServiceQueries>();
        _handler = new GetAllServicesQueryHandler(_queriesMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllServices()
    {
        // Arrange
        var query = new GetAllServicesQuery(ActiveOnly: false);
        var categoryId = Guid.NewGuid();
        var services = new List<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service>
        {
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Service 1").Build(),
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Service 2").Build(),
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Service 3").Build()
        };

        _queriesMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);

        _queriesMock.Verify(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithActiveOnlyTrue_ShouldReturnOnlyActiveServices()
    {
        // Arrange
        var query = new GetAllServicesQuery(ActiveOnly: true);
        var categoryId = Guid.NewGuid();
        var services = new List<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service>
        {
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Active 1").AsActive().Build(),
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Active 2").AsActive().Build()
        };

        _queriesMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(s => s.IsActive);

        _queriesMock.Verify(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoServices_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllServicesQuery(ActiveOnly: false);

        _queriesMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service>());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _queriesMock.Verify(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNameFilter_ShouldReturnMatchingServices()
    {
        // Arrange
        var query = new GetAllServicesQuery(Name: "Electrician");
        var categoryId = Guid.NewGuid();
        var services = new List<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service>
        {
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Electrician").Build(),
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Plumber").Build(),
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Auto Electrician").Build()
        };

        _queriesMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2); // Electrician & Auto Electrician originally contain "Electrician"
        result.Value.Select(s => s.Name).Should().Contain("Electrician");
        result.Value.Select(s => s.Name).Should().Contain("Auto Electrician");
        result.Value.Select(s => s.Name).Should().NotContain("Plumber");

        _queriesMock.Verify(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNameFilter_ShouldBeCaseInsensitive()
    {
        // Arrange
        var query = new GetAllServicesQuery(Name: "electrician"); // Lowercase query
        var categoryId = Guid.NewGuid();
        var services = new List<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service>
        {
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Electrician").Build(),
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Auto Electrician").Build(),
            new ServiceBuilder().WithCategoryId(categoryId).WithName("Plumber").Build()
        };

        _queriesMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(services);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(s => s.Name).Should().Contain("Electrician");
        result.Value.Select(s => s.Name).Should().Contain("Auto Electrician");

        _queriesMock.Verify(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }
}
