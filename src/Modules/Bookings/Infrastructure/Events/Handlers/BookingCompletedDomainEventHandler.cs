using MeAjudaAi.Modules.Bookings.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Bookings;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Events.Handlers;

internal sealed class BookingCompletedDomainEventHandler(
    IMessageBus messageBus,
    ILogger<BookingCompletedDomainEventHandler> logger)
    : IEventHandler<BookingCompletedDomainEvent>
{
    public async Task HandleAsync(BookingCompletedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var integrationEvent = new BookingCompletedIntegrationEvent(
                "Bookings",
                domainEvent.AggregateId,
                domainEvent.ProviderId,
                domainEvent.ClientId);

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            logger.LogInformation("Published BookingCompletedIntegrationEvent for booking {BookingId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing BookingCompletedIntegrationEvent for booking {BookingId}", domainEvent.AggregateId);
            throw;
        }
    }
}
