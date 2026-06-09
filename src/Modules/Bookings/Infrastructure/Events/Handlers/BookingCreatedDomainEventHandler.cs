using MeAjudaAi.Modules.Bookings.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Bookings;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Events.Handlers;

public sealed class BookingCreatedDomainEventHandler(
    IMessageBus messageBus,
    ILogger<BookingCreatedDomainEventHandler> logger)
    : IEventHandler<BookingCreatedDomainEvent>
{
    public async Task HandleAsync(BookingCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var integrationEvent = new BookingCreatedIntegrationEvent(
                "Bookings",
                domainEvent.AggregateId,
                domainEvent.ProviderId,
                domainEvent.ClientId,
                domainEvent.ServiceId,
                domainEvent.Date);

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            logger.LogInformation("Published BookingCreatedIntegrationEvent for booking {BookingId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing BookingCreatedIntegrationEvent for booking {BookingId}", domainEvent.AggregateId);
            throw;
        }
    }
}
