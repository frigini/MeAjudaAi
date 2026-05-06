using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class UpdateServiceCategoryCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IServiceCategoryQueries> _categoryQueriesMock;
    private readonly Mock<IRepository<ServiceCategory, ServiceCategoryId>> _repositoryMock;
    private readonly Mock<ILogger<UpdateServiceCategoryCommandHandler>> _loggerMock;
    private readonly UpdateServiceCategoryCommandHandler _handler;

    public UpdateServiceCategoryCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _categoryQueriesMock = new Mock<IServiceCategoryQueries>();
        _repositoryMock = new Mock<IRepository<ServiceCategory, ServiceCategoryId>>();
        _loggerMock = new Mock<ILogger<UpdateServiceCategoryCommandHandler>>();
        
        _uowMock.Setup(x => x.GetRepository<ServiceCategory, ServiceCategoryId>())
            .Returns(_repositoryMock.Object);
        
        _categoryQueriesMock.Setup(x => x.ExistsWithNameAsync(It.IsAny<string>(), It.IsAny<ServiceCategoryId?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
            
        _handler = new UpdateServiceCategoryCommandHandler(_uowMock.Object, _categoryQueriesMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        var category = new ServiceCategoryBuilder()
            .WithName("Categoria Original")
            .Build();
        var command = new UpdateServiceCategoryCommand(category.Id.Value, "Categoria Atualizada", "Desc", 1);

        _repositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        category.Name.Should().Be("Categoria Atualizada");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldReturnFailure()
    {
        var command = new UpdateServiceCategoryCommand(Guid.NewGuid(), "Nome", "Desc", 1);

        _repositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceCategory?)null);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithEmptyId_ShouldReturnFailure()
    {
        var command = new UpdateServiceCategoryCommand(Guid.Empty, "Nome", "Desc", 1);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnFailure()
    {
        var category = new ServiceCategoryBuilder().WithName("A").Build();
        var cmd = new UpdateServiceCategoryCommand(category.Id.Value, "A", "desc", 1);

        _repositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _categoryQueriesMock.Setup(x => x.ExistsWithNameAsync("A", category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(cmd);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenDbUpdateUniqueViolation_ShouldReturnFailureWithDuplicateMessage()
    {
        var category = new ServiceCategoryBuilder().WithName("A").Build();
        var cmd = new UpdateServiceCategoryCommand(category.Id.Value, "B", "desc", 1);

        _repositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Força a exceção no SaveChangesAsync
        var inner = new Exception("duplicate key value violates unique constraint ix_service_categories_name");
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateException("err", inner));

        var result = await _handler.HandleAsync(cmd);

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Contain("Já existe uma categoria");
    }
}
