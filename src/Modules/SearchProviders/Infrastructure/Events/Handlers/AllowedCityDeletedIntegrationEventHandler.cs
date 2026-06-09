using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Locations;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;

[ExcludeFromCodeCoverage]
public sealed class AllowedCityDeletedIntegrationEventHandler(
    ILogger<AllowedCityDeletedIntegrationEventHandler> logger) 
    : IEventHandler<AllowedCityDeletedIntegrationEvent>
{
    public Task HandleAsync(AllowedCityDeletedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling AllowedCityDeleted for city {CityId}", integrationEvent.CityId);
        return Task.CompletedTask;
    }
}
