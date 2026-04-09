using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MeAjudaAi.Modules.Communications.Application.Handlers;

/// <summary>
/// Handler para notificar o prestador quando seu status de verificação é atualizado.
/// </summary>
internal sealed class ProviderVerificationStatusUpdatedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ILogger<ProviderVerificationStatusUpdatedIntegrationEventHandler> logger)
    : IEventHandler<ProviderVerificationStatusUpdatedIntegrationEvent>
{
    public async Task HandleAsync(ProviderVerificationStatusUpdatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        // TODO: Resolver o e-mail do prestador via IUsersModuleApi usando integrationEvent.UserId
        string recipientEmail = "provider-email-lookup-pending@meajudaai.com.br";

        var emailPayload = new
        {
            To = recipientEmail,
            Subject = $"Atualização no status de verificação: {integrationEvent.NewStatus}",
            Body = $"Olá {integrationEvent.Name}, seu status de verificação foi alterado para: {integrationEvent.NewStatus}. {integrationEvent.Comments}",
            TemplateKey = integrationEvent.NewStatus.ToLower() == "approved" 
                ? "provider-verification-approved" 
                : "provider-verification-rejected"
        };

        var message = OutboxMessage.Create(
            ECommunicationChannel.Email,
            JsonSerializer.Serialize(emailPayload),
            ECommunicationPriority.High);

        await outboxRepository.AddAsync(message, cancellationToken);
        await outboxRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Verification status update notification enqueued for user {UserId}.", integrationEvent.UserId);
    }
}
