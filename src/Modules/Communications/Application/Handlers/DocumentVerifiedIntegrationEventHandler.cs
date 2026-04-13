using MeAjudaAi.Contracts.Modules.Providers;
using OutboxMessage = MeAjudaAi.Modules.Communications.Domain.Entities.OutboxMessage;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Documents;
using MeAjudaAi.Contracts.Shared;
using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Database.Outbox;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MeAjudaAi.Modules.Communications.Application.Handlers;

/// <summary>
/// Handler para notificar o prestador quando um documento é verificado com sucesso.
/// </summary>
public sealed class DocumentVerifiedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    IProvidersModuleApi providersModuleApi,
    ILogger<DocumentVerifiedIntegrationEventHandler> logger)
    : IEventHandler<DocumentVerifiedIntegrationEvent>
{
    public async Task HandleAsync(DocumentVerifiedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var providerResult = await providersModuleApi.GetProviderByIdAsync(integrationEvent.ProviderId, cancellationToken);
        
        if (!providerResult.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to fetch provider {integrationEvent.ProviderId} for document verified notification: {providerResult.Error.Message}");
        }

        if (providerResult.Value == null || string.IsNullOrWhiteSpace(providerResult.Value.Email))
        {
            logger.LogWarning(
                "Could not resolve email for provider {ProviderId}. Skipping document verified notification for document {DocumentId}.",
                integrationEvent.ProviderId, integrationEvent.DocumentId);
            return;
        }

        var recipientEmail = providerResult.Value.Email;
        var templateKey = CommunicationTemplateKeys.DocumentVerified;
        var correlationId = $"document_verified:{integrationEvent.DocumentId}:{integrationEvent.ProviderId}";

        var templateData = new Dictionary<string, string>
        {
            ["ProviderName"] = providerResult.Value.Name,
            ["DocumentType"] = integrationEvent.DocumentType
        };

        var emailPayload = new EmailOutboxPayload(
            To: recipientEmail,
            Subject: $"Documento verificado: {integrationEvent.DocumentType}",
            Body: $"Olá {providerResult.Value.Name}, seu documento ({integrationEvent.DocumentType}) foi verificado com sucesso.",
            TemplateKey: templateKey,
            TemplateData: templateData
        );

        var message = OutboxMessage.Create(
            channel: ECommunicationChannel.Email,
            payload: JsonSerializer.Serialize(emailPayload),
            priority: ECommunicationPriority.Normal,
            correlationId: correlationId);

        try
        {
            await outboxRepository.AddAsync(message, cancellationToken);
            await outboxRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Document verified notification enqueued for provider {ProviderId} (Email: {Email}, correlationId: {CorrelationId}).", 
                integrationEvent.ProviderId, PiiMaskingHelper.MaskEmail(recipientEmail), correlationId);
        }
        catch (Exception ex)
        {
            if (ex is Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                var processedException = MeAjudaAi.Shared.Database.Exceptions.PostgreSqlExceptionProcessor.ProcessException(dbEx);

                if (processedException is MeAjudaAi.Shared.Database.Exceptions.UniqueConstraintException uniqueEx && 
                    uniqueEx.ConstraintName == OutboxMessageConstraints.CorrelationIdIndexName)
                {
                    logger.LogInformation(
                        "Skipping document verified notification for document {DocumentId} — already enqueued (correlationId: {CorrelationId}).",
                        integrationEvent.DocumentId, correlationId);
                    return;
                }
            }
            
            throw;
        }
    }
}
