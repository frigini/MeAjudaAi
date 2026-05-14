using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class CreateServiceCategoryCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IServiceCategoryQueries> _categoryQueriesMock;
    private readonly Mock<IRepository<ServiceCategory, ServiceCategoryId>> _repositoryMock;
    private readonly CreateServiceCategoryCommandHandler _handler;

    public CreateServiceCategoryCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _categoryQueriesMock = new Mock<IServiceCategoryQueries>();
        _repositoryMock = new Mock<IRepository<ServiceCategory, ServiceCategoryId>>();
        
        _uowMock.Setup(x => x.GetRepository<ServiceCategory, ServiceCategoryId>())
            .Returns(_repositoryMock.Object);
        
        _categoryQueriesMock.Setup(x => x.ExistsWithNameAsync(It.IsAny<string>(), It.IsAny<ServiceCategoryId?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        _handler = new CreateServiceCategoryCommandHandler(_uowMock.Object, _categoryQueriesMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        var command = new CreateServiceCategoryCommand("Limpeza", "Serviços de limpeza", 1);

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnFailure()
    {
        var command = new CreateServiceCategoryCommand("Limpeza", "Serviços de limpeza", 1);
        _categoryQueriesMock.Setup(x => x.ExistsWithNameAsync("Limpeza", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Contain("Já existe");
    }

    [Fact]
    public async Task Handle_WhenDbUpdateUniqueViolation_ShouldReturnFailure()
    {
        var command = new CreateServiceCategoryCommand("Limpeza", "Serviços de limpeza", 1);
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateException("Database error", new Exception("unique constraint ix_service_categories_name")));

        var result = await _handler.HandleAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Contain("Já existe");
    }

    [Fact]
    public async Task Handle_WithInvalidDescription_ShouldReturnFailure()
    {
        // Descrição > 500 chars deve disparar erro de validação (result.IsFailure) ou Exception de domínio capturada pelo handler
        var invalidDescription = new string('a', 501);
        var command = new CreateServiceCategoryCommand("Valid Name", invalidDescription, 1);
        
        var result = await _handler.HandleAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Contain("não pode exceder 500 caracteres");
    }
}
