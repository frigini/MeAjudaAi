using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Contracts.Modules.Users;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Database.Exceptions;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Utilities;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Application.Handlers.Events;

/// <summary>
/// Handler para notificar o prestador quando seu status de verificação é atualizado.
/// </summary>
public sealed class ProviderVerificationStatusUpdatedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    IUsersModuleApi usersModuleApi,
    [FromKeyedServices(SerializationKeys.Api)] ISerializer serializer,
    ILogger<ProviderVerificationStatusUpdatedIntegrationEventHandler> logger)
    : IEventHandler<ProviderVerificationStatusUpdatedIntegrationEvent>
{
    public async Task HandleAsync(ProviderVerificationStatusUpdatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var userResult = await usersModuleApi.GetUserByIdAsync(integrationEvent.UserId, cancellationToken);
        
        if (!userResult.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to fetch user {integrationEvent.UserId} for provider {integrationEvent.ProviderId} verification update: {userResult.Error.Message}");
        }

        if (userResult.Value == null || string.IsNullOrWhiteSpace(userResult.Value.Email))
        {
            logger.LogWarning(
                "Could not resolve email for user {UserId}. Skipping verification status notification for provider {ProviderId}.",
                integrationEvent.UserId, integrationEvent.ProviderId);
            return;
        }

        var recipientEmail = userResult.Value.Email;
        var normalizedStatus = integrationEvent.NewStatus.Trim().ToLowerInvariant();
        
        var templateKey = normalizedStatus switch
        {
            "verified" or "approved" => CommunicationTemplateKeys.ProviderVerificationApproved,
            "rejected" or "denied" => CommunicationTemplateKeys.ProviderVerificationRejected,
            "pending" or "awaiting" => CommunicationTemplateKeys.ProviderVerificationPending,
            _ => CommunicationTemplateKeys.ProviderVerificationStatusUpdate
        };

        var displayStatus = normalizedStatus switch
        {
            "verified" or "approved" => "aprovado",
            "rejected" or "denied" => "rejeitado",
            "pending" or "awaiting" => "pendente",
            _ => normalizedStatus
        };

        var correlationId = $"{CommunicationTemplateKeys.ProviderVerificationStatusUpdate}{CommunicationConstants.CorrelationSeparator}{integrationEvent.Id}{CommunicationConstants.CorrelationSeparator}{integrationEvent.ProviderId}{CommunicationConstants.CorrelationSeparator}{normalizedStatus}";

        var emailPayload = new
        {
            To = recipientEmail,
            Subject = $"Atualização no status de verificação: {displayStatus}",
            Body = $"Olá {integrationEvent.Name}, seu status de verificação foi alterado para: {displayStatus}. {integrationEvent.Comments}",
            TemplateKey = templateKey,
            CorrelationId = correlationId
        };

        var message = OutboxMessage.Create(
            channel: ECommunicationChannel.Email,
            payload: serializer.Serialize(emailPayload),
            maxRetries: 3,
            priority: ECommunicationPriority.High,
            correlationId: correlationId);

        try
        {
            await outboxRepository.AddAsync(message, cancellationToken);
            await outboxRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Verification status update notification enqueued for user {UserId} ({Email}, correlationId: {CorrelationId}).", 
                integrationEvent.UserId, PiiMaskingHelper.MaskEmail(recipientEmail), correlationId);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            var processedException = PostgreSqlExceptionProcessor.ProcessException(ex);

            if (processedException is UniqueConstraintException)
            {
                logger.LogInformation(
                    "Skipping verification status update for provider {ProviderId} — already enqueued (correlationId: {CorrelationId}).",
                    integrationEvent.ProviderId, correlationId);
                return;
            }
            
            throw;
        }
    }
}
