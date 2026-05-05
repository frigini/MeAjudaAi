using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using Service = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service;
using ServiceCategory = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Contracts.Functional;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class CreateServiceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IServiceCategoryQueries> _categoryQueriesMock;
    private readonly Mock<IServiceQueries> _serviceQueriesMock;
    private readonly Mock<IRepository<ServiceCategory, ServiceCategoryId>> _categoryRepositoryMock;
    private readonly Mock<IRepository<Service, ServiceId>> _serviceRepositoryMock;
    private readonly CreateServiceCommandHandler _handler;

    public CreateServiceCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _categoryQueriesMock = new Mock<IServiceCategoryQueries>();
        _serviceQueriesMock = new Mock<IServiceQueries>();
        _categoryRepositoryMock = new Mock<IRepository<ServiceCategory, ServiceCategoryId>>();
        _serviceRepositoryMock = new Mock<IRepository<Service, ServiceId>>();
        
        _uowMock.Setup(u => u.GetRepository<ServiceCategory, ServiceCategoryId>()).Returns(_categoryRepositoryMock.Object);
        _uowMock.Setup(u => u.GetRepository<Service, ServiceId>()).Returns(_serviceRepositoryMock.Object);

        _handler = new CreateServiceCommandHandler(_uowMock.Object, _categoryQueriesMock.Object, _serviceQueriesMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var command = new CreateServiceCommand(category.Id.Value, "Limpeza de Piscina", "Limpeza profunda", 1);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _serviceQueriesMock.Setup(x => x.ExistsWithNameAsync(It.IsAny<string>(), null, category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBe(Guid.Empty);
        result.Value.Name.Should().Be(command.Name);
        result.Value.CategoryId.Should().Be(command.CategoryId);
        _serviceRepositoryMock.Verify(x => x.Add(It.IsAny<Service>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldReturnFailure()
    {
        var categoryId = Guid.NewGuid();
        var command = new CreateServiceCommand(categoryId, "Service Name", "Description", 1);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(ServiceCategoryId.From(categoryId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceCategory?)null);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Contain("não encontrada");
        _serviceRepositoryMock.Verify(x => x.Add(It.IsAny<Service>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInactiveCategory_ShouldReturnFailure()
    {
        var category = new ServiceCategoryBuilder().AsInactive().Build();
        var command = new CreateServiceCommand(category.Id.Value, "Limpeza de Piscina", "Limpeza profunda", 1);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Contain("inativa");
        _serviceRepositoryMock.Verify(x => x.Add(It.IsAny<Service>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnFailure()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var command = new CreateServiceCommand(category.Id.Value, "Existing Service", "Description", 1);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _serviceQueriesMock.Setup(x => x.ExistsWithNameAsync("Existing Service", null, category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Contain("Já existe um serviço com o nome");
        _serviceRepositoryMock.Verify(x => x.Add(It.IsAny<Service>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyCategoryId_ShouldReturnFailure()
    {
        var command = new CreateServiceCommand(Guid.Empty, "Service Name", "Description", 1);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("O identificador é obrigatório.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithEmptyName_ShouldReturnFailure(string? emptyName)
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var command = new CreateServiceCommand(category.Id.Value, emptyName!, "Description", 1);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("O nome do serviço é obrigatório.");
        _serviceRepositoryMock.Verify(x => x.Add(It.IsAny<Service>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNegativeDisplayOrder_ShouldReturnFailure()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var command = new CreateServiceCommand(category.Id.Value, "Service Name", "Description", -1);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("não pode ser negativa");
        _serviceRepositoryMock.Verify(x => x.Add(It.IsAny<Service>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDomainExceptionThrown_ShouldReturnFailureWithMessage()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var command = new CreateServiceCommand(category.Id.Value, "Valid Name", "Description", 1);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _serviceRepositoryMock
            .Setup(x => x.Add(It.IsAny<Service>()))
            .Throws(new CatalogDomainException("Domain rule violation"));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Domain rule violation");
    }
}
