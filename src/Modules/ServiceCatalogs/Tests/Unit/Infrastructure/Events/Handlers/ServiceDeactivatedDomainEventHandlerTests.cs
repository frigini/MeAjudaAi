using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Events.Handlers;
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
public class ServiceDeactivatedDomainEventHandlerTests
{
    private readonly Mock<IMessageBus> _messageBusMock = new();
    private readonly Mock<ILogger<ServiceDeactivatedDomainEventHandler>> _loggerMock = new();

    [Fact]
    public async Task ServiceDeactivatedHandler_Should_PublishIntegrationEvent()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var handler = new ServiceDeactivatedDomainEventHandler(_messageBusMock.Object, _loggerMock.Object);
        var domainEvent = new ServiceDeactivatedDomainEvent(ServiceId.From(serviceId));

        // Act
        await handler.HandleAsync(domainEvent);

        // Assert
        _messageBusMock.Verify(x => x.PublishAsync(
            It.Is<ServiceDeactivatedIntegrationEvent>(e => e.ServiceId == serviceId && e.Source == ModuleNames.ServiceCatalogs), 
            It.IsAny<string?>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ServiceDeactivatedHandler_Should_PropagateException_When_PublishFails()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        _messageBusMock.Setup(x => x.PublishAsync(
                It.IsAny<ServiceDeactivatedIntegrationEvent>(), 
                It.IsAny<string?>(), 
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Bus failure"));

        var handler = new ServiceDeactivatedDomainEventHandler(_messageBusMock.Object, _loggerMock.Object);
        var domainEvent = new ServiceDeactivatedDomainEvent(ServiceId.From(serviceId));

        // Act
        var act = () => handler.HandleAsync(domainEvent);

        // Assert
        await act.Should().ThrowExactlyAsync<InvalidOperationException>().WithMessage("Bus failure");
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error handling ServiceDeactivatedDomainEvent")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
