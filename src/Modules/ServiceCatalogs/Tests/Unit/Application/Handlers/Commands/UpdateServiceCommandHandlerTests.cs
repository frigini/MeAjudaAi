using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Database;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class UpdateServiceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Service, ServiceId>> _serviceRepositoryMock;
    private readonly Mock<IServiceQueries> _serviceQueriesMock;
    private readonly UpdateServiceCommandHandler _handler;

    public UpdateServiceCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _serviceRepositoryMock = new Mock<IRepository<Service, ServiceId>>();
        _serviceQueriesMock = new Mock<IServiceQueries>();

        _uowMock.Setup(x => x.GetRepository<Service, ServiceId>()).Returns(_serviceRepositoryMock.Object);

        _handler = new UpdateServiceCommandHandler(_uowMock.Object, _serviceQueriesMock.Object);
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var command = new UpdateServiceCommand(serviceId, "Updated Name", "Updated Description", 2);

        _serviceRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be(ValidationMessages.NotFound.Service);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyId_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateServiceCommand(Guid.Empty, "Name", "Description", 1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be(ValidationMessages.Required.Id);
        _serviceRepositoryMock.Verify(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnFailure()
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Original Name")
            .Build();
        var command = new UpdateServiceCommand(service.Id.Value, "Duplicate Name", "Description", 2);

        _serviceRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _serviceQueriesMock
            .Setup(x => x.ExistsWithNameAsync(command.Name, It.IsAny<ServiceId>(), It.IsAny<ServiceCategoryId?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        // Usando mensagem de validação genérica conforme observado nas falhas
        result.Error!.Message.Should().Be(string.Format(ValidationMessages.Catalogs.ServiceNameExists, command.Name));
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Handle_WithInvalidName_ShouldReturnFailure(string? invalidName)
    {
        // Arrange
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Valid Name")
            .Build();
        var command = new UpdateServiceCommand(service.Id.Value, invalidName!, "Description", 1);

        _serviceRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDomainExceptionThrown_ShouldReturnFailureWithMessage()
    {
        // Arrange
        var service = new ServiceBuilder().Build();
        var command = new UpdateServiceCommand(service.Id.Value, "Valid Name", "Description", 1);

        _serviceRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _serviceQueriesMock
            .Setup(x => x.ExistsWithNameAsync(It.IsAny<string>(), It.IsAny<ServiceId>(), It.IsAny<ServiceCategoryId?>(), It.IsAny<CancellationToken>()))
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
}
