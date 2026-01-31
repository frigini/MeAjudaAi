using FluentAssertions;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Exceptions;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Application.Handlers.Commands.Service;

public class CreateServiceCommandHandlerTests
{
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly Mock<IServiceCategoryRepository> _categoryRepositoryMock;
    private readonly CreateServiceCommandHandler _handler;

    public CreateServiceCommandHandlerTests()
    {
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _categoryRepositoryMock = new Mock<IServiceCategoryRepository>();
        _handler = new CreateServiceCommandHandler(_serviceRepositoryMock.Object, _categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenCategoryIdIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateServiceCommand(Guid.Empty, "Service Name", "Description", 1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Category ID cannot be empty");
    }

    [Fact]
    public async Task HandleAsync_WhenCategoryNotFound_ShouldThrowUnprocessableEntityException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateServiceCommand(categoryId, "Service Name", "Description", 1);

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceCategory?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .Where(e => e.Message.Contains("não encontrada"));
    }

    [Fact]
    public async Task HandleAsync_WhenCategoryInactive_ShouldThrowUnprocessableEntityException()
    {
        // Arrange
        var category = ServiceCategory.Create("Inactive Category", "Desc");
        category.Deactivate();

        var command = new CreateServiceCommand(category.Id.Value, "Service Name", "Description", 1);
        
        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .Where(e => e.Message.Contains("categoria inativa"));
    }

    [Fact]
    public async Task HandleAsync_WhenNameIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var category = ServiceCategory.Create("Category", "Desc");
        var command = new CreateServiceCommand(category.Id.Value, "", "Description", 1);

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Service name is required");
    }

    [Fact]
    public async Task HandleAsync_WhenServiceWithSameNameExists_ShouldReturnFailure()
    {
        // Arrange
        var category = ServiceCategory.Create("Category", "Desc");
        var command = new CreateServiceCommand(category.Id.Value, "Service Name", "Description", 1);

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _serviceRepositoryMock.Setup(r => r.ExistsWithNameAsync(It.IsAny<string>(), null, It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("already exists in this category");
    }

    [Fact]
    public async Task HandleAsync_WhenDisplayOrderIsNegative_ShouldReturnFailure()
    {
        // Arrange
        var category = ServiceCategory.Create("Category", "Desc");
        var command = new CreateServiceCommand(category.Id.Value, "Service Name", "Description", -1);

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Display order cannot be negative");
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldCreateServiceAndReturnSuccess()
    {
        // Arrange
        var category = ServiceCategory.Create("Category", "Desc");
        var command = new CreateServiceCommand(category.Id.Value, "Service Name", "Description", 1);

        _categoryRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _serviceRepositoryMock.Setup(r => r.ExistsWithNameAsync(It.IsAny<string>(), null, It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Service Name");
        result.Value.CategoryName.Should().Be("Category");
        
        _serviceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
