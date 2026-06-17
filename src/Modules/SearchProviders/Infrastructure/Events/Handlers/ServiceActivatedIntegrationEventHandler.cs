using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;

internal sealed class ServiceActivatedIntegrationEventHandler(
    ISearchProvidersModuleApi searchProvidersModuleApi,
    IProvidersModuleApi providersModuleApi,
    ILogger<ServiceActivatedIntegrationEventHandler> logger) : IEventHandler<ServiceActivatedIntegrationEvent>
{
    public async Task HandleAsync(ServiceActivatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Handling ServiceActivatedIntegrationEvent for service {ServiceId} ({Name}). Reindexing providers...",
            integrationEvent.ServiceId,
            integrationEvent.Name);

        // Busca todos os prestadores que oferecem este serviço
        var providersResult = await providersModuleApi.GetProvidersByServiceAsync(integrationEvent.ServiceId, cancellationToken);
        
        if (providersResult.IsFailure)
        {
            logger.LogError("Failed to retrieve providers for service {ServiceId}: {Error}", integrationEvent.ServiceId, providersResult.Error);
            return;
        }

        foreach (var providerId in providersResult.Value)
        {
            var indexResult = await searchProvidersModuleApi.IndexProviderAsync(providerId, cancellationToken);
            if (indexResult.IsFailure)
            {
                logger.LogError("Failed to reindex provider {ProviderId} after service activation: {Error}", providerId, indexResult.Error);
            }
        }
        
        logger.LogInformation("Finished processing ServiceActivatedIntegrationEvent for service {ServiceId}.", integrationEvent.ServiceId);
    }
}
