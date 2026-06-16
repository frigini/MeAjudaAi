using MeAjudaAi.Modules.Locations.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Locations;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Events.Handlers;

internal sealed class AllowedCityDeletedDomainEventHandler(
    IMessageBus messageBus,
    ILogger<AllowedCityDeletedDomainEventHandler> logger)
    : IEventHandler<AllowedCityDeletedDomainEvent>
{
    public async Task HandleAsync(AllowedCityDeletedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var integrationEvent = new AllowedCityDeletedIntegrationEvent(
                "Locations",
                domainEvent.CityId);

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            logger.LogInformation("Published AllowedCityDeletedIntegrationEvent for city {CityId}", domainEvent.CityId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing AllowedCityDeletedIntegrationEvent for city {CityId}", domainEvent.CityId);
            throw;
        }
    }
}
