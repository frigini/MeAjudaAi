using MeAjudaAi.Modules.Payments.Domain.Events;
using MeAjudaAi.Modules.Payments.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.Payments.Tests.Unit.Infrastructure.Events.Handlers;

[Trait("Category", "Unit")]
[Trait("Module", "Payments")]
[Trait("Layer", "Infrastructure")]
public class SubscriptionDomainEventHandlersTests
{
    private readonly Mock<IMessageBus> _messageBusMock = new();
    private readonly Mock<ILogger<SubscriptionActivatedDomainEventHandler>> _loggerActivatedMock = new();
    private readonly Mock<ILogger<SubscriptionCanceledDomainEventHandler>> _loggerCanceledMock = new();
    private readonly Mock<ILogger<SubscriptionExpiredDomainEventHandler>> _loggerExpiredMock = new();
    private readonly Mock<ILogger<SubscriptionRenewedDomainEventHandler>> _loggerRenewedMock = new();

    [Fact]
    public async Task ActivatedHandler_ShouldPublishIntegrationEvent()
    {
        var handler = new SubscriptionActivatedDomainEventHandler(_messageBusMock.Object, _loggerActivatedMock.Object);
        var domainEvent = new SubscriptionActivatedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), "ext_id", 1);

        await handler.HandleAsync(domainEvent, CancellationToken.None);

        _messageBusMock.Verify(m => m.PublishAsync(
            It.Is<SubscriptionActivatedIntegrationEvent>(e => e.SubscriptionId == domainEvent.SubscriptionId && e.UserId == domainEvent.ProviderId),
            null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CanceledHandler_ShouldPublishIntegrationEvent()
    {
        var handler = new SubscriptionCanceledDomainEventHandler(_messageBusMock.Object, _loggerCanceledMock.Object);
        var domainEvent = new SubscriptionCanceledDomainEvent(Guid.NewGuid(), Guid.NewGuid(), 1);

        await handler.HandleAsync(domainEvent, CancellationToken.None);

        _messageBusMock.Verify(m => m.PublishAsync(
            It.Is<SubscriptionCanceledIntegrationEvent>(e => e.SubscriptionId == domainEvent.SubscriptionId && e.UserId == domainEvent.ProviderId),
            null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExpiredHandler_ShouldPublishIntegrationEvent()
    {
        var handler = new SubscriptionExpiredDomainEventHandler(_messageBusMock.Object, _loggerExpiredMock.Object);
        var domainEvent = new SubscriptionExpiredDomainEvent(Guid.NewGuid(), Guid.NewGuid(), 1);

        await handler.HandleAsync(domainEvent, CancellationToken.None);

        _messageBusMock.Verify(m => m.PublishAsync(
            It.Is<SubscriptionExpiredIntegrationEvent>(e => e.SubscriptionId == domainEvent.SubscriptionId && e.UserId == domainEvent.ProviderId),
            null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RenewedHandler_ShouldPublishIntegrationEvent()
    {
        var handler = new SubscriptionRenewedDomainEventHandler(_messageBusMock.Object, _loggerRenewedMock.Object);
        var domainEvent = new SubscriptionRenewedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddMonths(1), 1);

        await handler.HandleAsync(domainEvent, CancellationToken.None);

        _messageBusMock.Verify(m => m.PublishAsync(
            It.Is<SubscriptionRenewedIntegrationEvent>(e => e.SubscriptionId == domainEvent.SubscriptionId && e.UserId == domainEvent.ProviderId && e.NewExpiresAt == domainEvent.NewExpiresAt),
            null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
