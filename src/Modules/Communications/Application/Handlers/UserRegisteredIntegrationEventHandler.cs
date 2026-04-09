using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using MeAjudaAi.Contracts.Shared;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MeAjudaAi.Modules.Communications.Application.Handlers;

/// <summary>
/// Consome o evento UserRegisteredIntegrationEvent e enfileira e-mail de boas-vindas no Outbox.
/// </summary>
internal sealed class UserRegisteredIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogRepository logRepository,
    ILogger<UserRegisteredIntegrationEventHandler> logger)
    : IEventHandler<UserRegisteredIntegrationEvent>
{
    private const string TemplateKey = "user_registered";

    public async Task HandleAsync(
        UserRegisteredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"{TemplateKey}:{integrationEvent.UserId}";

        // Idempotência: evita re-enfileirar se já foi processado
        if (await logRepository.ExistsByCorrelationIdAsync(correlationId, cancellationToken))
        {
            logger.LogInformation(
                "Skipping welcome email for user {UserId} — already sent (correlationId: {CorrelationId}).",
                integrationEvent.UserId, correlationId);
            return;
        }

        var payload = JsonSerializer.Serialize(new
        {
            To = integrationEvent.Email,
            Subject = "Bem-vindo ao MeAjudaAi!",
            HtmlBody = $"<h1>Olá, {integrationEvent.FirstName}!</h1><p>Seja bem-vindo(a) ao MeAjudaAi.</p>",
            TextBody = $"Olá, {integrationEvent.FirstName}!\nSeja bem-vindo(a) ao MeAjudaAi.",
            From = (string?)null,
            CorrelationId = correlationId,
            TemplateKey = TemplateKey
        });

        var message = OutboxMessage.Create(
            channel: ECommunicationChannel.Email,
            payload: payload,
            priority: ECommunicationPriority.Normal,
            correlationId: correlationId);

        try
        {
            await outboxRepository.AddAsync(message, cancellationToken);
            await outboxRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Welcome email enqueued for user {UserId} (outboxId: {OutboxId}, correlationId: {CorrelationId}).",
                integrationEvent.UserId, message.Id, correlationId);
        }
        catch (Exception ex) when (ex.Message.Contains("duplicate key") || ex.InnerException?.Message.Contains("duplicate key") == true)
        {
            logger.LogInformation(
                "Skipping welcome email for user {UserId} — already enqueued or sent (correlationId: {CorrelationId}).",
                integrationEvent.UserId, correlationId);
        }
    }
}
