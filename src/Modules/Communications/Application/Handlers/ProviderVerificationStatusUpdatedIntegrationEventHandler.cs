using MeAjudaAi.Contracts.Modules.Users;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Contracts.Shared;
using MeAjudaAi.Shared.Utilities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MeAjudaAi.Modules.Communications.Application.Handlers;

/// <summary>
/// Handler para notificar o prestador quando seu status de verificação é atualizado.
/// </summary>
public sealed class ProviderVerificationStatusUpdatedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    IUsersModuleApi usersModuleApi,
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
            "verified" or "approved" => "provider-verification-approved",
            "rejected" or "denied" => "provider-verification-rejected",
            "pending" or "awaiting" => "provider-verification-pending",
            _ => "provider-verification-status-update" // default/fallback
        };

        var displayStatus = normalizedStatus switch
        {
            "verified" or "approved" => "aprovado",
            "rejected" or "denied" => "rejeitado",
            "pending" or "awaiting" => "pendente",
            _ => normalizedStatus
        };

        var correlationId = $"verification_status_update:{integrationEvent.Id}:{integrationEvent.ProviderId}:{normalizedStatus}";

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
            payload: JsonSerializer.Serialize(emailPayload),
            priority: ECommunicationPriority.High,
            correlationId: correlationId);

        try
        {
            await outboxRepository.AddAsync(message, cancellationToken);
            await outboxRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Verification status update notification enqueued for user {UserId} ({Email}, correlationId: {CorrelationId}).", 
                integrationEvent.UserId, PiiMaskingHelper.MaskEmail(recipientEmail), correlationId);
        }
        catch (Exception ex)
        {
            var processedException = MeAjudaAi.Shared.Database.Exceptions.PostgreSqlExceptionProcessor.ProcessException(
                ex as Microsoft.EntityFrameworkCore.DbUpdateException ?? new Microsoft.EntityFrameworkCore.DbUpdateException(ex.Message, ex));

            if (processedException is MeAjudaAi.Shared.Database.Exceptions.UniqueConstraintException)
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
