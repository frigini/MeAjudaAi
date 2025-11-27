using MeAjudaAi.Shared.Contracts.Modules.SearchProviders;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;

/// <summary>
/// Handler para processar eventos de integração de prestadores ativados.
/// </summary>
/// <remarks>
/// Este handler é acionado quando um prestador é ativado no módulo Providers.
/// Indexa o prestador no módulo de busca para que fique disponível nas pesquisas.
/// </remarks>
public sealed class ProviderActivatedIntegrationEventHandler(
    ISearchProvidersModuleApi SearchProvidersModuleApi,
    ILogger<ProviderActivatedIntegrationEventHandler> logger) : IEventHandler<ProviderActivatedIntegrationEvent>
{
    /// <summary>
    /// Processa o evento de prestador ativado e indexa no módulo de busca.
    /// </summary>
    /// <param name="integrationEvent">Evento de integração com dados do prestador</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    public async Task HandleAsync(ProviderActivatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Handling ProviderActivatedIntegrationEvent for provider {ProviderId}, indexing in search",
                integrationEvent.ProviderId);

            // Indexa o provider no módulo de busca
            var indexResult = await SearchProvidersModuleApi.IndexProviderAsync(
                integrationEvent.ProviderId,
                cancellationToken);

            if (indexResult.IsFailure)
            {
                logger.LogError(
                    "Failed to index provider {ProviderId} in search: {Error}",
                    integrationEvent.ProviderId,
                    indexResult.Error.Message);
                
                // NOTA: Não propagamos a exceção porque o provider já foi ativado com sucesso.
                // A indexação pode ser refeita posteriormente via retry mechanism ou comando manual.
                // Log de erro é suficiente para alertar sobre o problema.
                return;
            }

            logger.LogInformation(
                "Provider {ProviderId} ({Name}) successfully indexed in search module after activation",
                integrationEvent.ProviderId,
                integrationEvent.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error handling ProviderActivatedIntegrationEvent for provider {ProviderId}",
                integrationEvent.ProviderId);
            
            // Mesma lógica: não propagamos erro para não falhar a transação do módulo Providers
            // A indexação é uma operação secundária que pode ser refeita
        }
    }
}
