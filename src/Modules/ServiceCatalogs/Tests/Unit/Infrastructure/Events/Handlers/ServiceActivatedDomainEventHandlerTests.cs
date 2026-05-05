using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Infrastructure.Events.Handlers;

[Trait("Category", "Unit")]
public class ServiceActivatedDomainEventHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IRepository<Service, ServiceId>> _serviceRepositoryMock = new();
    private readonly Mock<IMessageBus> _messageBusMock = new();
    private readonly Mock<ILogger<ServiceActivatedDomainEventHandler>> _loggerMock = new();

    public ServiceActivatedDomainEventHandlerTests()
    {
        _uowMock.Setup(x => x.GetRepository<Service, ServiceId>()).Returns(_serviceRepositoryMock.Object);
    }

    [Fact]
    public async Task ServiceActivatedHandler_Should_PublishIntegrationEvent()
    {
        // Arrange
        var service = Service.Create(ServiceCategoryId.From(Guid.NewGuid()), "Test Service", null, 0);
        _serviceRepositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        var handler = new ServiceActivatedDomainEventHandler(_uowMock.Object, _messageBusMock.Object, _loggerMock.Object);
        var domainEvent = new ServiceActivatedDomainEvent(service.Id);

        // Act
        await handler.HandleAsync(domainEvent);

        // Assert
        _messageBusMock.Verify(x => x.PublishAsync(
            It.Is<ServiceActivatedIntegrationEvent>(e => 
                e.ServiceId == service.Id.Value && 
                e.Name == service.Name && 
                e.Source == ModuleNames.ServiceCatalogs), 
            It.IsAny<string?>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ServiceActivatedHandler_Should_Throw_When_ServiceNotFound()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        _serviceRepositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        var handler = new ServiceActivatedDomainEventHandler(_uowMock.Object, _messageBusMock.Object, _loggerMock.Object);
        var domainEvent = new ServiceActivatedDomainEvent(ServiceId.From(serviceId));

        // Act
        var act = () => handler.HandleAsync(domainEvent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error handling ServiceActivatedDomainEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _messageBusMock.Verify(x => x.PublishAsync(It.IsAny<ServiceActivatedIntegrationEvent>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ServiceActivatedHandler_Should_PropagateException_When_PublishFails()
    {
        // Arrange
        var service = Service.Create(ServiceCategoryId.From(Guid.NewGuid()), "Test Service", null, 0);
        _serviceRepositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _messageBusMock.Setup(x => x.PublishAsync(
                It.IsAny<ServiceActivatedIntegrationEvent>(), 
                It.IsAny<string?>(), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("publish-failed"));

        var handler = new ServiceActivatedDomainEventHandler(_uowMock.Object, _messageBusMock.Object, _loggerMock.Object);
        var domainEvent = new ServiceActivatedDomainEvent(service.Id);

        // Act
        var act = () => handler.HandleAsync(domainEvent);

        // Assert
        await act.Should().ThrowExactlyAsync<InvalidOperationException>().WithMessage("publish-failed");
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error handling ServiceActivatedDomainEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
