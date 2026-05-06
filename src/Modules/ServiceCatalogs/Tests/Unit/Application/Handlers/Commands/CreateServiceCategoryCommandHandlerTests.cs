using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;
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
    public async Task Handle_WithEmptyName_ShouldReturnFailure()
    {
        var command = new CreateServiceCategoryCommand("", "Description", 1);

        var result = await _handler.HandleAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnFailure()
    {
        _categoryQueriesMock
            .Setup(x => x.ExistsWithNameAsync("Limpeza", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateServiceCategoryCommand("Limpeza", "Serviços de limpeza", 1);

        var result = await _handler.HandleAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Contain("Limpeza");
        _repositoryMock.Verify(x => x.Add(It.IsAny<ServiceCategory>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDomainExceptionThrown_ShouldReturnFailure()
    {
        _repositoryMock
            .Setup(x => x.Add(It.IsAny<ServiceCategory>()))
            .Throws(new CatalogDomainException("Domain rule violation"));

        var command = new CreateServiceCategoryCommand("Limpeza", "Serviços de limpeza", 1);

        var result = await _handler.HandleAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be("Domain rule violation");
    }

    [Fact]
    public async Task Handle_WhenDbUpdateExceptionWithUniqueConstraint_ShouldReturnFailure()
    {
        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Update failed", new Exception("unique constraint violated")));

        var command = new CreateServiceCategoryCommand("Limpeza", "Serviços de limpeza", 1);

        var result = await _handler.HandleAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Contain("Limpeza");
    }
}
