using MeAjudaAi.Modules.Locations.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Locations;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Events.Handlers;

public sealed class AllowedCityCreatedDomainEventHandler(
    IMessageBus messageBus,
    ILogger<AllowedCityCreatedDomainEventHandler> logger)
    : IEventHandler<AllowedCityCreatedDomainEvent>
{
    public async Task HandleAsync(AllowedCityCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var integrationEvent = new AllowedCityCreatedIntegrationEvent(
                "Locations",
                domainEvent.CityId,
                domainEvent.CityName,
                domainEvent.StateSigla);

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            logger.LogInformation("Published AllowedCityCreatedIntegrationEvent for city {CityId}", domainEvent.CityId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing AllowedCityCreatedIntegrationEvent for city {CityId}", domainEvent.CityId);
            throw;
        }
    }
}
