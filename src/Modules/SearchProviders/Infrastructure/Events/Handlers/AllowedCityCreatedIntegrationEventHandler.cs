using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Locations;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;

public sealed class AllowedCityCreatedIntegrationEventHandler(
    ILogger<AllowedCityCreatedIntegrationEventHandler> logger) 
    : IEventHandler<AllowedCityCreatedIntegrationEvent>
{
    public Task HandleAsync(AllowedCityCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling AllowedCityCreated for {CityName}", integrationEvent.CityName);
        // Implementar reindexação regional quando filtragem por cidade estiver pronta
        return Task.CompletedTask;
    }
}
