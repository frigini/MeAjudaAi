using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Contracts.Utilities.Constants;
using Microsoft.Extensions.Logging.Abstractions;

using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Application.Handlers.Commands.Service;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class UpdateServiceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service, ServiceId>> _serviceRepositoryMock;
    private readonly Mock<IServiceQueries> _serviceQueriesMock;
    private readonly UpdateServiceCommandHandler _handler;

    public UpdateServiceCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _serviceRepositoryMock = new Mock<IRepository<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service, ServiceId>>();
        _serviceQueriesMock = new Mock<IServiceQueries>();
        
        _uowMock.Setup(u => u.GetRepository<MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service, ServiceId>())
            .Returns(_serviceRepositoryMock.Object);
            
        _handler = new UpdateServiceCommandHandler(_uowMock.Object, _serviceQueriesMock.Object, NullLogger<UpdateServiceCommandHandler>.Instance);
    }

    [Fact]
    public async Task HandleAsync_WhenIdIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateServiceCommand(Guid.Empty, "Name", "Desc", 1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(ValidationMessages.Required.Id);
    }

    [Fact]
    public async Task HandleAsync_WhenServiceNotFound_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var command = new UpdateServiceCommand(serviceId, "Name", "Desc", 1);

        _serviceRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_WhenNameIsEmpty_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var command = new UpdateServiceCommand(serviceId, "", "Desc", 1);

        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Original Name")
            .Build();
        
        _serviceRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(ValidationMessages.Required.ServiceName);
    }

    [Fact]
    public async Task HandleAsync_WhenNameExists_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var command = new UpdateServiceCommand(serviceId, "New Name", "Desc", 1);

        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Original Name")
            .Build();

        _serviceRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _serviceQueriesMock.Setup(r => r.ExistsWithNameAsync(It.IsAny<string>(), It.IsAny<ServiceId>(), It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be(string.Format(ValidationMessages.Catalogs.ServiceNameExists, "New Name"));
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ShouldUpdateAndReturnSuccess()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var command = new UpdateServiceCommand(serviceId, "Updated Name", "Updated Desc", 2);

        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Original Name")
            .Build();

        _serviceRepositoryMock.Setup(r => r.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _serviceQueriesMock.Setup(r => r.ExistsWithNameAsync(It.IsAny<string>(), It.IsAny<ServiceId>(), It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        service.Name.Should().Be("Updated Name");
        service.Description.Should().Be("Updated Desc");
        service.DisplayOrder.Should().Be(2);

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}



