using FluentAssertions;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Exceptions;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Application.Handlers.Commands.Service;

public class CreateServiceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<ServiceCategory, ServiceCategoryId>> _categoryRepositoryMock;
    private readonly Mock<IRepository<Domain.Entities.Service, ServiceId>> _serviceRepositoryMock;
    private readonly CreateServiceCommandHandler _handler;

    public CreateServiceCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _categoryRepositoryMock = new Mock<IRepository<ServiceCategory, ServiceCategoryId>>();
        _serviceRepositoryMock = new Mock<IRepository<Domain.Entities.Service, ServiceId>>();
        
        _uowMock.Setup(u => u.GetRepository<ServiceCategory, ServiceCategoryId>()).Returns(_categoryRepositoryMock.Object);
        _uowMock.Setup(u => u.GetRepository<Domain.Entities.Service, ServiceId>()).Returns(_serviceRepositoryMock.Object);
        
        _handler = new CreateServiceCommandHandler(_uowMock.Object);
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

        _categoryRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceCategory?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .WithMessage("*não encontrada*");
        _serviceRepositoryMock.Verify(x => x.Add(It.IsAny<Domain.Entities.Service>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenCategoryInactive_ShouldThrowUnprocessableEntityException()
    {
        // Arrange
        var category = ServiceCategory.Create("Inactive Category", "Desc");
        category.Deactivate();

        var command = new CreateServiceCommand(category.Id.Value, "Service Name", "Description", 1);
        
        _categoryRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var act = async () => await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnprocessableEntityException>()
            .WithMessage("*inativa*");
        _serviceRepositoryMock.Verify(x => x.Add(It.IsAny<Domain.Entities.Service>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenNameIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var category = ServiceCategory.Create("Category", "Desc");
        var command = new CreateServiceCommand(category.Id.Value, "", "Description", 1);

        _categoryRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("O nome do serviço é obrigatório.");
    }

    [Fact(Skip = "Awaiting implementation of IServiceQueries - duplicate name validation pending")]
    public async Task HandleAsync_WhenServiceWithSameNameExists_ShouldReturnFailure()
    {
        // ... test body ...
    }

    [Fact]
    public async Task HandleAsync_WhenDisplayOrderIsNegative_ShouldReturnFailure()
    {
        // Arrange
        var category = ServiceCategory.Create("Category", "Desc");
        var command = new CreateServiceCommand(category.Id.Value, "Service Name", "Description", -1);

        _categoryRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
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

        _categoryRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Service Name");
        result.Value.CategoryName.Should().Be("Category");
        
        _serviceRepositoryMock.Verify(r => r.Add(It.IsAny<Domain.Entities.Service>()), Times.Once);
    }
}
