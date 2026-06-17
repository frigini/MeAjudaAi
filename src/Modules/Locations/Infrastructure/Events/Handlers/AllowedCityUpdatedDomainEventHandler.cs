using MeAjudaAi.Modules.Locations.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Locations;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Events.Handlers;

internal sealed class AllowedCityUpdatedDomainEventHandler(
    IMessageBus messageBus,
    ILogger<AllowedCityUpdatedDomainEventHandler> logger)
    : IEventHandler<AllowedCityUpdatedDomainEvent>
{
    public async Task HandleAsync(AllowedCityUpdatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var integrationEvent = new AllowedCityUpdatedIntegrationEvent(
                "Locations",
                domainEvent.CityId,
                domainEvent.CityName,
                domainEvent.StateSigla);

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            logger.LogInformation("Published AllowedCityUpdatedIntegrationEvent for city {CityId}", domainEvent.CityId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing AllowedCityUpdatedIntegrationEvent for city {CityId}", domainEvent.CityId);
            throw;
        }
    }
}
