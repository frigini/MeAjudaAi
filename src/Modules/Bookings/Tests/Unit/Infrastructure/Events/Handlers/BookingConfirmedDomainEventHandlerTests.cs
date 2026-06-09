using MeAjudaAi.Modules.Bookings.Domain.Events;
using MeAjudaAi.Modules.Bookings.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Bookings;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Infrastructure.Events.Handlers;

public class BookingConfirmedDomainEventHandlerTests
{
    private readonly Mock<IMessageBus> _messageBusMock = new();
    private readonly Mock<ILogger<BookingConfirmedDomainEventHandler>> _loggerMock = new();
    private readonly BookingConfirmedDomainEventHandler _handler;

    public BookingConfirmedDomainEventHandlerTests()
    {
        _handler = new BookingConfirmedDomainEventHandler(_messageBusMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldPublishIntegrationEvent()
    {
        var domainEvent = new BookingConfirmedDomainEvent(Guid.NewGuid(), 1, Guid.NewGuid(), Guid.NewGuid());

        await _handler.HandleAsync(domainEvent);

        _messageBusMock.Verify(
            x => x.PublishAsync(It.IsAny<BookingConfirmedIntegrationEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenPublishFails_ShouldRethrowException()
    {
        var domainEvent = new BookingConfirmedDomainEvent(Guid.NewGuid(), 1, Guid.NewGuid(), Guid.NewGuid());
        
        _messageBusMock
            .Setup(x => x.PublishAsync(It.IsAny<BookingConfirmedIntegrationEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Bus unavailable"));

        Func<Task> act = () => _handler.HandleAsync(domainEvent);

        await act.Should().ThrowAsync<Exception>().WithMessage("Bus unavailable");
    }
}
