using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Infrastructure.Events.Handlers;

[Trait("Category", "Unit")]
public class ServiceEventHandlersTests
{
    private readonly Mock<IServiceRepository> _serviceRepositoryMock = new();
    private readonly Mock<IMessageBus> _messageBusMock = new();
    private readonly Mock<ILogger<ServiceActivatedDomainEventHandler>> _activatedLoggerMock = new();
    private readonly Mock<ILogger<ServiceDeactivatedDomainEventHandler>> _deactivatedLoggerMock = new();

    [Fact]
    public async Task ServiceActivatedHandler_Should_PublishIntegrationEvent()
    {
        // Arrange
        var service = Service.Create(ServiceCategoryId.From(Guid.NewGuid()), "Test Service", null, 0);
        _serviceRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        var handler = new ServiceActivatedDomainEventHandler(_serviceRepositoryMock.Object, _messageBusMock.Object, _activatedLoggerMock.Object);
        var domainEvent = new ServiceActivatedDomainEvent(service.Id);

        // Act
        await handler.HandleAsync(domainEvent);

        // Assert
        _messageBusMock.Verify(x => x.PublishAsync(It.Is<ServiceActivatedIntegrationEvent>(e => e.ServiceId == service.Id.Value && e.Name == service.Name), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ServiceActivatedHandler_Should_Throw_When_ServiceNotFound()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        _serviceRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        var handler = new ServiceActivatedDomainEventHandler(_serviceRepositoryMock.Object, _messageBusMock.Object, _activatedLoggerMock.Object);
        var domainEvent = new ServiceActivatedDomainEvent(ServiceId.From(serviceId));

        // Act
        var act = () => handler.HandleAsync(domainEvent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        _messageBusMock.Verify(x => x.PublishAsync(It.IsAny<ServiceActivatedIntegrationEvent>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ServiceDeactivatedHandler_Should_PublishIntegrationEvent()
{
    // Arrange
    var serviceId = Guid.NewGuid();
    // IServiceRepository is not needed for this handler

    var handler = new ServiceDeactivatedDomainEventHandler(_messageBusMock.Object, _deactivatedLoggerMock.Object);
    var domainEvent = new ServiceDeactivatedDomainEvent(ServiceId.From(serviceId));

    // Act
    await handler.HandleAsync(domainEvent);

    // Assert
    _messageBusMock.Verify(x => x.PublishAsync(It.Is<ServiceDeactivatedIntegrationEvent>(e => e.ServiceId == serviceId), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
}
}

