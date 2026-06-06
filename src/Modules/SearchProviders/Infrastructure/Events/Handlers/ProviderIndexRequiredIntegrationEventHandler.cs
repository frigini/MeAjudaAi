using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;

/// <summary>
/// Handler de integração que reage ao evento de necessidade de indexação de prestador.
/// </summary>
public sealed class ProviderIndexRequiredIntegrationEventHandler(
    ISearchProvidersModuleApi searchProvidersModuleApi,
    ILogger<ProviderIndexRequiredIntegrationEventHandler> logger) : IEventHandler<ProviderIndexRequiredIntegrationEvent>
{
    public async Task HandleAsync(ProviderIndexRequiredIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling ProviderIndexRequiredIntegrationEvent for provider {ProviderId}", integrationEvent.ProviderId);

        var indexResult = await searchProvidersModuleApi.IndexProviderAsync(integrationEvent.ProviderId, cancellationToken);
        
        if (indexResult.IsFailure)
        {
            logger.LogError("Failed to index provider {ProviderId} in search: {Error}",
                integrationEvent.ProviderId, indexResult.Error);
        }
        else
        {
            logger.LogInformation("Provider {ProviderId} indexed in SearchProviders successfully", integrationEvent.ProviderId);
        }
    }
}
