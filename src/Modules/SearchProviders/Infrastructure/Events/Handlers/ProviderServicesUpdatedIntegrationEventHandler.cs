using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;

/// <summary>
/// Handler para processar eventos de integração quando os serviços de um prestador são atualizados.
/// </summary>
/// <remarks>
/// Este handler é acionado quando serviços são adicionados/removidos de um prestador no módulo Providers.
/// Reindexа o prestador no módulo de busca para refletir as alterações.
/// </remarks>
public sealed class ProviderServicesUpdatedIntegrationEventHandler(
    ISearchProvidersModuleApi SearchProvidersModuleApi,
    ILogger<ProviderServicesUpdatedIntegrationEventHandler> logger) : IEventHandler<ProviderServicesUpdatedIntegrationEvent>
{
    /// <summary>
    /// Processa o evento de serviços atualizados e reindexа o prestador.
    /// </summary>
    /// <param name="integrationEvent">Evento de integração com dados dos serviços</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    public async Task HandleAsync(ProviderServicesUpdatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Handling ProviderServicesUpdatedIntegrationEvent for provider {ProviderId}, re-indexing in search",
                integrationEvent.ProviderId);

            // Reindexа o provider completo no módulo de busca
            // Isso vai buscar todos os dados atualizados incluindo a lista de serviços
            var indexResult = await SearchProvidersModuleApi.IndexProviderAsync(
                integrationEvent.ProviderId,
                cancellationToken);

            if (indexResult.IsFailure)
            {
                logger.LogError(
                    "Failed to re-index provider {ProviderId} in search after services update: {Error}",
                    integrationEvent.ProviderId,
                    indexResult.Error.Message);

                // NOTA: Não propagamos a exceção porque os serviços já foram atualizados com sucesso.
                // A reindexação pode ser refeita posteriormente via retry mechanism ou comando manual.
                return;
            }

            logger.LogInformation(
                "Provider {ProviderId} successfully re-indexed in search module after services update",
                integrationEvent.ProviderId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error handling ProviderServicesUpdatedIntegrationEvent for provider {ProviderId}",
                integrationEvent.ProviderId);

            // Não propagamos a exceção
        }
    }
}
