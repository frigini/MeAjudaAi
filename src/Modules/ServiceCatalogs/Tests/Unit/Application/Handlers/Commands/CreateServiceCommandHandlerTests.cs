using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.ServiceCatalogs;
using Microsoft.Extensions.Logging.Abstractions;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class CreateServiceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Service, ServiceId>> _serviceRepositoryMock;
    private readonly Mock<IServiceQueries> _serviceQueriesMock;
    private readonly Mock<IServiceCategoryQueries> _categoryQueriesMock;
    private readonly CreateServiceCommandHandler _handler;

    public CreateServiceCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _serviceRepositoryMock = new Mock<IRepository<Service, ServiceId>>();
        _serviceQueriesMock = new Mock<IServiceQueries>();
        _categoryQueriesMock = new Mock<IServiceCategoryQueries>();

        _uowMock.Setup(x => x.GetRepository<Service, ServiceId>()).Returns(_serviceRepositoryMock.Object);

        _handler = new CreateServiceCommandHandler(
            _uowMock.Object,
            _serviceQueriesMock.Object,
            _categoryQueriesMock.Object,
            NullLogger<CreateServiceCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var command = new CreateServiceCommand(category.Id.Value, "Limpeza de Piscina", "Limpeza profunda", 1);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _serviceQueriesMock
            .Setup(x => x.ExistsWithNameAsync(command.Name, null, It.IsAny<ServiceCategoryId?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBe(Guid.Empty);
        result.Value.Name.Should().Be(command.Name);
        result.Value.CategoryId.Should().Be(command.CategoryId);
        _serviceRepositoryMock.Verify(x => x.Add(It.IsAny<Service>()), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldReturnFailure()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateServiceCommand(categoryId, "Service Name", "Description", 1);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceCategory?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .WithMessage("*não encontrada*");
        _serviceRepositoryMock.Verify(x => x.Add(It.IsAny<Service>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInactiveCategory_ShouldReturnFailure()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsInactive().Build();
        var command = new CreateServiceCommand(category.Id.Value, "Limpeza de Piscina", "Limpeza profunda", 1);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .WithMessage("*inativa*");
        _serviceRepositoryMock.Verify(x => x.Add(It.IsAny<Service>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnFailure()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var command = new CreateServiceCommand(category.Id.Value, "Duplicate Name", "Description", 1);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _serviceQueriesMock
            .Setup(x => x.ExistsWithNameAsync(command.Name, null, It.IsAny<ServiceCategoryId?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("already exists");
        _serviceRepositoryMock.Verify(x => x.Add(It.IsAny<Service>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyCategoryId_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateServiceCommand(Guid.Empty, "Service Name", "Description", 1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("cannot be empty");
        _categoryQueriesMock.Verify(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithEmptyName_ShouldReturnFailure(string? emptyName)
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var command = new CreateServiceCommand(category.Id.Value, emptyName!, "Description", 1);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("required");
        _serviceRepositoryMock.Verify(x => x.Add(It.IsAny<Service>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNegativeDisplayOrder_ShouldReturnFailure()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var command = new CreateServiceCommand(category.Id.Value, "Service Name", "Description", -1);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _serviceQueriesMock
            .Setup(x => x.ExistsWithNameAsync(It.IsAny<string>(), null, It.IsAny<ServiceCategoryId?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("cannot be negative");
        _serviceRepositoryMock.Verify(x => x.Add(It.IsAny<Service>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDomainExceptionThrown_ShouldReturnFailureWithMessage()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var command = new CreateServiceCommand(category.Id.Value, "Valid Name", "Description", 1);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _serviceQueriesMock
            .Setup(x => x.ExistsWithNameAsync(It.IsAny<string>(), null, It.IsAny<ServiceCategoryId?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CatalogDomainException("Domain rule violation"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Domain rule violation");
    }

    [Fact]
    public async Task Handle_WhenGenericExceptionThrown_ShouldReturnGenericFailure()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var command = new CreateServiceCommand(category.Id.Value, "Valid Name", "Description", 1);

        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _serviceQueriesMock
            .Setup(x => x.ExistsWithNameAsync(It.IsAny<string>(), null, It.IsAny<ServiceCategoryId?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected DB error"));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Ocorreu um erro inesperado ao criar o serviço.");
    }
}