using MeAjudaAi.Modules.Bookings.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Bookings;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Events.Handlers;

public sealed class BookingCancelledDomainEventHandler(
    IMessageBus messageBus,
    ILogger<BookingCancelledDomainEventHandler> logger)
    : IEventHandler<BookingCancelledDomainEvent>
{
    public async Task HandleAsync(BookingCancelledDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var integrationEvent = new BookingCancelledIntegrationEvent(
                "Bookings",
                domainEvent.AggregateId,
                domainEvent.ProviderId,
                domainEvent.ClientId,
                domainEvent.Reason);

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            logger.LogInformation("Published BookingCancelledIntegrationEvent for booking {BookingId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing BookingCancelledIntegrationEvent for booking {BookingId}", domainEvent.AggregateId);
            throw;
        }
    }
}
