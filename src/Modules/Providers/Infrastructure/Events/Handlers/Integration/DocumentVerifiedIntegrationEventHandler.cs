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

            // Usa consulta leve para obter apenas o status sem carregar a entidade completa
            var (exists, status) = await providerRepository.GetProviderStatusAsync(
                new ProviderId(integrationEvent.ProviderId),
                cancellationToken);

            if (!exists)
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
                status);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error handling DocumentVerifiedIntegrationEvent for provider {ProviderId}, document {DocumentId}",
                integrationEvent.ProviderId,
                integrationEvent.DocumentId);
            throw new InvalidOperationException(
                $"Failed to process DocumentVerified integration event for provider '{integrationEvent.ProviderId}', document '{integrationEvent.DocumentId}'",
                ex);
        }
    }
}
