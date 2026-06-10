using MeAjudaAi.Modules.Bookings.Domain.Events;
using MeAjudaAi.Modules.Bookings.Infrastructure.Events.Handlers;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Bookings;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Tests.Unit.Infrastructure.Events.Handlers;

public class BookingCompletedDomainEventHandlerTests
{
    private readonly Mock<IMessageBus> _messageBusMock = new();
    private readonly Mock<ILogger<BookingCompletedDomainEventHandler>> _loggerMock = new();
    private readonly BookingCompletedDomainEventHandler _handler;

    public BookingCompletedDomainEventHandlerTests()
    {
        _handler = new BookingCompletedDomainEventHandler(_messageBusMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenCalled_ShouldPublishIntegrationEvent()
    {
        // Arrange
        var domainEvent = new BookingCompletedDomainEvent(Guid.NewGuid(), 1, Guid.NewGuid(), Guid.NewGuid());

        // Act
        await _handler.HandleAsync(domainEvent);

        // Assert
        _messageBusMock.Verify(
            x => x.PublishAsync(It.IsAny<BookingCompletedIntegrationEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenPublishFails_ShouldRethrowException()
    {
        // Arrange
        var domainEvent = new BookingCompletedDomainEvent(Guid.NewGuid(), 1, Guid.NewGuid(), Guid.NewGuid());
        
        _messageBusMock
            .Setup(x => x.PublishAsync(It.IsAny<BookingCompletedIntegrationEvent>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Bus unavailable"));

        // Act
        Func<Task> act = () => _handler.HandleAsync(domainEvent);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Bus unavailable");
    }
}
