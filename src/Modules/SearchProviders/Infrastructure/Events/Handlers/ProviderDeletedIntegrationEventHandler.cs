using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;

/// <summary>
/// Handler para processar eventos de exclusão de prestador.
/// </summary>
internal sealed class ProviderDeletedIntegrationEventHandler(
    ISearchProvidersModuleApi searchProvidersModuleApi,
    ILogger<ProviderDeletedIntegrationEventHandler> logger) : IEventHandler<ProviderDeletedIntegrationEvent>
{
    public async Task HandleAsync(ProviderDeletedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Handling ProviderDeletedIntegrationEvent for provider {ProviderId}",
                integrationEvent.ProviderId);

            // Remover o provider do índice de busca
            var result = await searchProvidersModuleApi.RemoveProviderAsync(integrationEvent.ProviderId, cancellationToken);

            if (result.IsFailure)
            {
                logger.LogError(
                    "Failed to remove provider {ProviderId} from search index after deletion: {Error}",
                    integrationEvent.ProviderId,
                    result.Error.Message);
            }
            else
            {
                logger.LogInformation(
                    "Provider {ProviderId} removed from search index successfully after deletion",
                    integrationEvent.ProviderId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error handling ProviderDeletedIntegrationEvent for provider {ProviderId}",
                integrationEvent.ProviderId);
        }
    }
}
