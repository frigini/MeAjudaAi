using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Documents;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers.Integration;

/// <summary>
/// Handler para processar eventos de integração de documentos verificados.
/// </summary>
/// <remarks>
/// Este handler é acionado quando um documento é verificado no módulo Documents.
/// Pode ser usado para atualizar flags ou métricas relacionadas à verificação de documentos
/// do prestador. A lógica de negócio principal (ativação do prestador) é tratada
/// separadamente via comandos explícitos.
/// </remarks>
public sealed class DocumentVerifiedIntegrationEventHandler(
    IProviderRepository providerRepository,
    ILogger<DocumentVerifiedIntegrationEventHandler> logger) : IEventHandler<DocumentVerifiedIntegrationEvent>
{
    /// <summary>
    /// Processa o evento de documento verificado.
    /// </summary>
    /// <param name="integrationEvent">Evento de integração com dados do documento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    public async Task HandleAsync(DocumentVerifiedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Handling DocumentVerifiedIntegrationEvent for provider {ProviderId}, document {DocumentId}",
                integrationEvent.ProviderId,
                integrationEvent.DocumentId);

            // Busca o provider para validar que existe
            var provider = await providerRepository.GetByIdAsync(
                new ProviderId(integrationEvent.ProviderId),
                cancellationToken);

            if (provider == null)
            {
                logger.LogWarning(
                    "Provider {ProviderId} not found when handling DocumentVerifiedIntegrationEvent for document {DocumentId}",
                    integrationEvent.ProviderId,
                    integrationEvent.DocumentId);
                return;
            }

            // NOTA: A ativação do provider é feita via comando explícito (ActivateProviderCommand)
            // Este handler apenas loga o evento para auditoria e métricas.
            // Futuramente, pode ser usado para:
            // - Atualizar contador de documentos verificados
            // - Enviar notificações ao prestador
            // - Atualizar dashboard de progresso

            logger.LogInformation(
                "Document {DocumentId} of type {DocumentType} verified for provider {ProviderId}. Provider status: {Status}",
                integrationEvent.DocumentId,
                integrationEvent.DocumentType,
                integrationEvent.ProviderId,
                provider.Status);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error handling DocumentVerifiedIntegrationEvent for provider {ProviderId}, document {DocumentId}",
                integrationEvent.ProviderId,
                integrationEvent.DocumentId);
            throw;
        }
    }
}
