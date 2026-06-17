using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;

/// <summary>
/// Handler para processar eventos de atualização de perfil de prestador.
/// </summary>
internal sealed class ProviderProfileUpdatedIntegrationEventHandler(
    ISearchProvidersModuleApi searchProvidersModuleApi,
    ILogger<ProviderProfileUpdatedIntegrationEventHandler> logger) : IEventHandler<ProviderProfileUpdatedIntegrationEvent>
{
    public async Task HandleAsync(ProviderProfileUpdatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Handling ProviderProfileUpdatedIntegrationEvent for provider {ProviderId}. Updated fields: {UpdatedFields}",
                integrationEvent.ProviderId,
                string.Join(", ", integrationEvent.UpdatedFields));

            // Reindexar o provider para garantir que os dados no índice estejam atualizados
            var result = await searchProvidersModuleApi.IndexProviderAsync(integrationEvent.ProviderId, cancellationToken);

            if (result.IsFailure)
            {
                logger.LogError(
                    "Failed to reindex provider {ProviderId} after profile update: {Error}",
                    integrationEvent.ProviderId,
                    result.Error.Message);
            }
            else
            {
                logger.LogInformation(
                    "Provider {ProviderId} reindexed successfully after profile update",
                    integrationEvent.ProviderId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error handling ProviderProfileUpdatedIntegrationEvent for provider {ProviderId}",
                integrationEvent.ProviderId);
        }
    }
}
