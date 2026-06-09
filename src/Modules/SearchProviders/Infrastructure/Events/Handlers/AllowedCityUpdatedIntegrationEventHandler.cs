using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Locations;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;

[ExcludeFromCodeCoverage]
public sealed class AllowedCityUpdatedIntegrationEventHandler(
    ILogger<AllowedCityUpdatedIntegrationEventHandler> logger) 
    : IEventHandler<AllowedCityUpdatedIntegrationEvent>
{
    public Task HandleAsync(AllowedCityUpdatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling AllowedCityUpdated for city {CityId}", integrationEvent.CityId);
        return Task.CompletedTask;
    }
}
