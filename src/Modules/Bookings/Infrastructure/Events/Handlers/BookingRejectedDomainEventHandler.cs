using MeAjudaAi.Modules.Bookings.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Bookings;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.Infrastructure.Events.Handlers;

internal sealed class BookingRejectedDomainEventHandler(
    IMessageBus messageBus,
    ILogger<BookingRejectedDomainEventHandler> logger)
    : IEventHandler<BookingRejectedDomainEvent>
{
    public async Task HandleAsync(BookingRejectedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var integrationEvent = new BookingRejectedIntegrationEvent(
                "Bookings",
                domainEvent.AggregateId,
                domainEvent.ProviderId,
                domainEvent.ClientId,
                domainEvent.Reason);

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            logger.LogInformation("Published BookingRejectedIntegrationEvent for booking {BookingId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing BookingRejectedIntegrationEvent for booking {BookingId}", domainEvent.AggregateId);
            throw;
        }
    }
}
