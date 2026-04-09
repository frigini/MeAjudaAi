using MeAjudaAi.Contracts.Modules.Users;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Contracts.Shared;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MeAjudaAi.Modules.Communications.Application.Handlers;

/// <summary>
/// Handler para notificar o prestador quando seu status de verificação é atualizado.
/// </summary>
internal sealed class ProviderVerificationStatusUpdatedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    IUsersModuleApi usersModuleApi,
    ILogger<ProviderVerificationStatusUpdatedIntegrationEventHandler> logger)
    : IEventHandler<ProviderVerificationStatusUpdatedIntegrationEvent>
{
    public async Task HandleAsync(ProviderVerificationStatusUpdatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var userResult = await usersModuleApi.GetUserByIdAsync(integrationEvent.UserId, cancellationToken);
        
        if (!userResult.IsSuccess || userResult.Value == null || string.IsNullOrWhiteSpace(userResult.Value.Email))
        {
            logger.LogWarning(
                "Could not resolve email for user {UserId}. Skipping verification status notification for provider {ProviderId}.",
                integrationEvent.UserId, integrationEvent.ProviderId);
            return;
        }

        var recipientEmail = userResult.Value.Email;
        var templateKey = integrationEvent.NewStatus.ToLower() == "approved" 
            ? "provider-verification-approved" 
            : "provider-verification-rejected";
        var correlationId = $"verification_status_update:{integrationEvent.ProviderId}:{integrationEvent.NewStatus}";

        var emailPayload = new
        {
            To = recipientEmail,
            Subject = $"Atualização no status de verificação: {integrationEvent.NewStatus}",
            Body = $"Olá {integrationEvent.Name}, seu status de verificação foi alterado para: {integrationEvent.NewStatus}. {integrationEvent.Comments}",
            TemplateKey = templateKey,
            CorrelationId = correlationId
        };

        var message = OutboxMessage.Create(
            ECommunicationChannel.Email,
            JsonSerializer.Serialize(emailPayload),
            ECommunicationPriority.High,
            correlationId: correlationId);

        try
        {
            await outboxRepository.AddAsync(message, cancellationToken);
            await outboxRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Verification status update notification enqueued for user {UserId} ({Email}, correlationId: {CorrelationId}).", 
                integrationEvent.UserId, recipientEmail, correlationId);
        }
        catch (Exception ex) when (ex.Message.Contains("duplicate key") || ex.InnerException?.Message.Contains("duplicate key") == true)
        {
            logger.LogInformation(
                "Skipping verification status update for provider {ProviderId} — already enqueued (correlationId: {CorrelationId}).",
                integrationEvent.ProviderId, correlationId);
        }
    }
}
