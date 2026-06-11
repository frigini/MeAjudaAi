using MeAjudaAi.Modules.Bookings.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Bookings;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Events.Handlers;

internal sealed class BookingConfirmedDomainEventHandler(
    IMessageBus messageBus,
    ILogger<BookingConfirmedDomainEventHandler> logger)
    : IEventHandler<BookingConfirmedDomainEvent>
{
    public async Task HandleAsync(BookingConfirmedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var integrationEvent = new BookingConfirmedIntegrationEvent(
                "Bookings",
                domainEvent.AggregateId,
                domainEvent.ProviderId,
                domainEvent.ClientId);

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            logger.LogInformation("Published BookingConfirmedIntegrationEvent for booking {BookingId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing BookingConfirmedIntegrationEvent for booking {BookingId}", domainEvent.AggregateId);
            throw;
        }
    }
}
