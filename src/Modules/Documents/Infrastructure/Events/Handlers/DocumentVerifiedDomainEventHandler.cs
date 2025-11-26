using MeAjudaAi.Modules.Documents.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Documents;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Events.Handlers;

/// <summary>
/// Manipula eventos de domínio DocumentVerifiedDomainEvent e publica eventos de integração.
/// </summary>
/// <remarks>
/// Responsável por converter eventos de domínio em eventos de integração para comunicação
/// entre módulos. Quando um documento é verificado com sucesso, este handler publica um evento
/// de integração para notificar o módulo Providers para atualizar o status do prestador.
/// </remarks>
public sealed class DocumentVerifiedDomainEventHandler(
    IMessageBus messageBus,
    ILogger<DocumentVerifiedDomainEventHandler> logger) : IEventHandler<DocumentVerifiedDomainEvent>
{
    private const string ModuleName = "Documents";

    /// <summary>
    /// Processa o evento de domínio de documento verificado de forma assíncrona.
    /// </summary>
    /// <param name="domainEvent">Evento de domínio contendo dados do documento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    public async Task HandleAsync(DocumentVerifiedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Handling DocumentVerifiedDomainEvent for document {DocumentId}, provider {ProviderId}",
                domainEvent.AggregateId,
                domainEvent.ProviderId);

            // Cria evento de integração para notificar outros módulos
            var integrationEvent = new DocumentVerifiedIntegrationEvent(
                Source: ModuleName,
                DocumentId: domainEvent.AggregateId,
                ProviderId: domainEvent.ProviderId,
                DocumentType: domainEvent.DocumentType.ToString(),
                HasOcrData: domainEvent.HasOcrData,
                VerifiedAt: DateTime.UtcNow
            );

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);

            logger.LogInformation(
                "Successfully published DocumentVerified integration event for document {DocumentId}",
                domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error handling DocumentVerifiedDomainEvent for document {DocumentId}",
                domainEvent.AggregateId);
            throw;
        }
    }
}
