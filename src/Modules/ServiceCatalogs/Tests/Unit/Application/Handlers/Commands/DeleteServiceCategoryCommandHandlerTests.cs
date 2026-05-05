using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Database;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class DeleteServiceCategoryCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IServiceQueries> _serviceQueriesMock;
    private readonly Mock<IRepository<ServiceCategory, ServiceCategoryId>> _categoryRepoMock;
    private readonly DeleteServiceCategoryCommandHandler _handler;

    public DeleteServiceCategoryCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _serviceQueriesMock = new Mock<IServiceQueries>();
        _categoryRepoMock = new Mock<IRepository<ServiceCategory, ServiceCategoryId>>();
        
        _uowMock.Setup(x => x.GetRepository<ServiceCategory, ServiceCategoryId>())
            .Returns(_categoryRepoMock.Object);
        
        _handler = new DeleteServiceCategoryCommandHandler(_uowMock.Object, _serviceQueriesMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        var category = new ServiceCategoryBuilder().WithName("Limpeza").Build();
        var command = new DeleteServiceCategoryCommand(category.Id.Value);

        _categoryRepoMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _serviceQueriesMock.Setup(x => x.CountByCategoryAsync(category.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _handler.HandleAsync(command, CancellationToken.None);
        
        result.IsSuccess.Should().BeTrue();
        _categoryRepoMock.Verify(x => x.Delete(It.Is<ServiceCategory>(c => c.Id == category.Id)), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldReturnFailure()
    {
        var command = new DeleteServiceCategoryCommand(Guid.NewGuid());

        _categoryRepoMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceCategory?)null);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithEmptyId_ShouldReturnFailure()
    {
        var command = new DeleteServiceCategoryCommand(Guid.Empty);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
