using MeAjudaAi.Modules.Payments.Domain.Events;
using MeAjudaAi.Modules.Payments.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using Microsoft.Extensions.Logging;

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

    #region Activated

    [Fact]
    public async Task ActivatedHandler_ShouldPublishIntegrationEvent()
    {
        var handler = new SubscriptionActivatedDomainEventHandler(_messageBusMock.Object, _loggerActivatedMock.Object);
        var domainEvent = new SubscriptionActivatedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), "ext_id", 1);

        await handler.HandleAsync(domainEvent, CancellationToken.None);

        _messageBusMock.Verify(m => m.PublishAsync(
            It.Is<SubscriptionActivatedIntegrationEvent>(e =>
                e.SubscriptionId == domainEvent.SubscriptionId &&
                e.UserId == domainEvent.ProviderId),
            null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActivatedHandler_ShouldLogInformation_WhenSucceeds()
    {
        var handler = new SubscriptionActivatedDomainEventHandler(_messageBusMock.Object, _loggerActivatedMock.Object);
        var domainEvent = new SubscriptionActivatedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), "ext_id", 1);

        await handler.HandleAsync(domainEvent, CancellationToken.None);

        _loggerActivatedMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ActivatedHandler_ShouldThrow_WhenMessageBusFails()
    {
        var handler = new SubscriptionActivatedDomainEventHandler(_messageBusMock.Object, _loggerActivatedMock.Object);
        var domainEvent = new SubscriptionActivatedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), "ext_id", 1);
        _messageBusMock.Setup(m => m.PublishAsync(It.IsAny<SubscriptionActivatedIntegrationEvent>(), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Bus error"));

        var act = () => handler.HandleAsync(domainEvent, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Be("Bus error");
    }

    [Fact]
    public async Task ActivatedHandler_ShouldLogError_WhenMessageBusFails()
    {
        var handler = new SubscriptionActivatedDomainEventHandler(_messageBusMock.Object, _loggerActivatedMock.Object);
        var domainEvent = new SubscriptionActivatedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), "ext_id", 1);
        _messageBusMock.Setup(m => m.PublishAsync(It.IsAny<SubscriptionActivatedIntegrationEvent>(), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Bus error"));

        Func<Task> act = () => handler.HandleAsync(domainEvent, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        _loggerActivatedMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ActivatedHandler_ShouldPropagateCancellationToken()
    {
        var handler = new SubscriptionActivatedDomainEventHandler(_messageBusMock.Object, _loggerActivatedMock.Object);
        var domainEvent = new SubscriptionActivatedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), "ext_id", 1);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        _messageBusMock.Setup(m => m.PublishAsync(It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var act = () => handler.HandleAsync(domainEvent, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Canceled

    [Fact]
    public async Task CanceledHandler_ShouldPublishIntegrationEvent()
    {
        var handler = new SubscriptionCanceledDomainEventHandler(_messageBusMock.Object, _loggerCanceledMock.Object);
        var domainEvent = new SubscriptionCanceledDomainEvent(Guid.NewGuid(), Guid.NewGuid(), 1);

        await handler.HandleAsync(domainEvent, CancellationToken.None);

        _messageBusMock.Verify(m => m.PublishAsync(
            It.Is<SubscriptionCanceledIntegrationEvent>(e =>
                e.SubscriptionId == domainEvent.SubscriptionId &&
                e.UserId == domainEvent.ProviderId),
            null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CanceledHandler_ShouldLogInformation_WhenSucceeds()
    {
        var handler = new SubscriptionCanceledDomainEventHandler(_messageBusMock.Object, _loggerCanceledMock.Object);
        var domainEvent = new SubscriptionCanceledDomainEvent(Guid.NewGuid(), Guid.NewGuid(), 1);

        await handler.HandleAsync(domainEvent, CancellationToken.None);

        _loggerCanceledMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CanceledHandler_ShouldThrow_WhenMessageBusFails()
    {
        var handler = new SubscriptionCanceledDomainEventHandler(_messageBusMock.Object, _loggerCanceledMock.Object);
        var domainEvent = new SubscriptionCanceledDomainEvent(Guid.NewGuid(), Guid.NewGuid(), 1);
        _messageBusMock.Setup(m => m.PublishAsync(It.IsAny<SubscriptionCanceledIntegrationEvent>(), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Bus error"));

        var act = () => handler.HandleAsync(domainEvent, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Be("Bus error");
    }

    [Fact]
    public async Task CanceledHandler_ShouldLogError_WhenMessageBusFails()
    {
        var handler = new SubscriptionCanceledDomainEventHandler(_messageBusMock.Object, _loggerCanceledMock.Object);
        var domainEvent = new SubscriptionCanceledDomainEvent(Guid.NewGuid(), Guid.NewGuid(), 1);
        _messageBusMock.Setup(m => m.PublishAsync(It.IsAny<SubscriptionCanceledIntegrationEvent>(), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Bus error"));

        Func<Task> act = () => handler.HandleAsync(domainEvent, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        _loggerCanceledMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CanceledHandler_ShouldPropagateCancellationToken()
    {
        var handler = new SubscriptionCanceledDomainEventHandler(_messageBusMock.Object, _loggerCanceledMock.Object);
        var domainEvent = new SubscriptionCanceledDomainEvent(Guid.NewGuid(), Guid.NewGuid(), 1);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        _messageBusMock.Setup(m => m.PublishAsync(It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var act = () => handler.HandleAsync(domainEvent, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Expired

    [Fact]
    public async Task ExpiredHandler_ShouldPublishIntegrationEvent()
    {
        var handler = new SubscriptionExpiredDomainEventHandler(_messageBusMock.Object, _loggerExpiredMock.Object);
        var domainEvent = new SubscriptionExpiredDomainEvent(Guid.NewGuid(), Guid.NewGuid(), 1);

        await handler.HandleAsync(domainEvent, CancellationToken.None);

        _messageBusMock.Verify(m => m.PublishAsync(
            It.Is<SubscriptionExpiredIntegrationEvent>(e =>
                e.SubscriptionId == domainEvent.SubscriptionId &&
                e.UserId == domainEvent.ProviderId),
            null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExpiredHandler_ShouldLogInformation_WhenSucceeds()
    {
        var handler = new SubscriptionExpiredDomainEventHandler(_messageBusMock.Object, _loggerExpiredMock.Object);
        var domainEvent = new SubscriptionExpiredDomainEvent(Guid.NewGuid(), Guid.NewGuid(), 1);

        await handler.HandleAsync(domainEvent, CancellationToken.None);

        _loggerExpiredMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExpiredHandler_ShouldThrow_WhenMessageBusFails()
    {
        var handler = new SubscriptionExpiredDomainEventHandler(_messageBusMock.Object, _loggerExpiredMock.Object);
        var domainEvent = new SubscriptionExpiredDomainEvent(Guid.NewGuid(), Guid.NewGuid(), 1);
        _messageBusMock.Setup(m => m.PublishAsync(It.IsAny<SubscriptionExpiredIntegrationEvent>(), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Bus error"));

        var act = () => handler.HandleAsync(domainEvent, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Be("Bus error");
    }

    [Fact]
    public async Task ExpiredHandler_ShouldLogError_WhenMessageBusFails()
    {
        var handler = new SubscriptionExpiredDomainEventHandler(_messageBusMock.Object, _loggerExpiredMock.Object);
        var domainEvent = new SubscriptionExpiredDomainEvent(Guid.NewGuid(), Guid.NewGuid(), 1);
        _messageBusMock.Setup(m => m.PublishAsync(It.IsAny<SubscriptionExpiredIntegrationEvent>(), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Bus error"));

        Func<Task> act = () => handler.HandleAsync(domainEvent, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        _loggerExpiredMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExpiredHandler_ShouldPropagateCancellationToken()
    {
        var handler = new SubscriptionExpiredDomainEventHandler(_messageBusMock.Object, _loggerExpiredMock.Object);
        var domainEvent = new SubscriptionExpiredDomainEvent(Guid.NewGuid(), Guid.NewGuid(), 1);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        _messageBusMock.Setup(m => m.PublishAsync(It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var act = () => handler.HandleAsync(domainEvent, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Renewed

    [Fact]
    public async Task RenewedHandler_ShouldPublishIntegrationEvent()
    {
        var handler = new SubscriptionRenewedDomainEventHandler(_messageBusMock.Object, _loggerRenewedMock.Object);
        var domainEvent = new SubscriptionRenewedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddMonths(1), 1);

        await handler.HandleAsync(domainEvent, CancellationToken.None);

        _messageBusMock.Verify(m => m.PublishAsync(
            It.Is<SubscriptionRenewedIntegrationEvent>(e =>
                e.SubscriptionId == domainEvent.SubscriptionId &&
                e.UserId == domainEvent.ProviderId &&
                e.NewExpiresAt == domainEvent.NewExpiresAt),
            null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RenewedHandler_ShouldLogInformation_WhenSucceeds()
    {
        var handler = new SubscriptionRenewedDomainEventHandler(_messageBusMock.Object, _loggerRenewedMock.Object);
        var domainEvent = new SubscriptionRenewedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddMonths(1), 1);

        await handler.HandleAsync(domainEvent, CancellationToken.None);

        _loggerRenewedMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task RenewedHandler_ShouldThrow_WhenMessageBusFails()
    {
        var handler = new SubscriptionRenewedDomainEventHandler(_messageBusMock.Object, _loggerRenewedMock.Object);
        var domainEvent = new SubscriptionRenewedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddMonths(1), 1);
        _messageBusMock.Setup(m => m.PublishAsync(It.IsAny<SubscriptionRenewedIntegrationEvent>(), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Bus error"));

        var act = () => handler.HandleAsync(domainEvent, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Be("Bus error");
    }

    [Fact]
    public async Task RenewedHandler_ShouldLogError_WhenMessageBusFails()
    {
        var handler = new SubscriptionRenewedDomainEventHandler(_messageBusMock.Object, _loggerRenewedMock.Object);
        var domainEvent = new SubscriptionRenewedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddMonths(1), 1);
        _messageBusMock.Setup(m => m.PublishAsync(It.IsAny<SubscriptionRenewedIntegrationEvent>(), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Bus error"));

        Func<Task> act = () => handler.HandleAsync(domainEvent, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();

        _loggerRenewedMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task RenewedHandler_ShouldPropagateCancellationToken()
    {
        var handler = new SubscriptionRenewedDomainEventHandler(_messageBusMock.Object, _loggerRenewedMock.Object);
        var domainEvent = new SubscriptionRenewedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddMonths(1), 1);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        _messageBusMock.Setup(m => m.PublishAsync(It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var act = () => handler.HandleAsync(domainEvent, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion
}
