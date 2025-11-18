using MeAjudaAi.Modules.Catalogs.Application.Handlers.Queries;
using MeAjudaAi.Modules.Catalogs.Application.Queries;
using MeAjudaAi.Modules.Catalogs.Domain.Repositories;
using MeAjudaAi.Modules.Catalogs.Tests.Builders;

namespace MeAjudaAi.Modules.Catalogs.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
[Trait("Module", "Catalogs")]
[Trait("Layer", "Application")]
public class GetAllServiceCategoriesQueryHandlerTests
{
    private readonly Mock<IServiceCategoryRepository> _repositoryMock;
    private readonly GetAllServiceCategoriesQueryHandler _handler;

    public GetAllServiceCategoriesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IServiceCategoryRepository>();
        _handler = new GetAllServiceCategoriesQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllCategories()
    {
        // Arrange
        var query = new GetAllServiceCategoriesQuery(ActiveOnly: false);
        var categories = new List<MeAjudaAi.Modules.Catalogs.Domain.Entities.ServiceCategory>
        {
            new ServiceCategoryBuilder().WithName("Limpeza").Build(),
            new ServiceCategoryBuilder().WithName("Reparos").Build(),
            new ServiceCategoryBuilder().WithName("Pintura").Build()
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value.Should().Contain(c => c.Name == "Limpeza");
        result.Value.Should().Contain(c => c.Name == "Reparos");
        result.Value.Should().Contain(c => c.Name == "Pintura");

        _repositoryMock.Verify(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithActiveOnlyTrue_ShouldReturnOnlyActiveCategories()
    {
        // Arrange
        var query = new GetAllServiceCategoriesQuery(ActiveOnly: true);
        var categories = new List<MeAjudaAi.Modules.Catalogs.Domain.Entities.ServiceCategory>
        {
            new ServiceCategoryBuilder().WithName("Limpeza").AsActive().Build(),
            new ServiceCategoryBuilder().WithName("Reparos").AsActive().Build()
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(c => c.IsActive);

        _repositoryMock.Verify(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoCategories_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllServiceCategoriesQuery(ActiveOnly: false);

        _repositoryMock
            .Setup(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MeAjudaAi.Modules.Catalogs.Domain.Entities.ServiceCategory>());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _repositoryMock.Verify(x => x.GetAllAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }
}
