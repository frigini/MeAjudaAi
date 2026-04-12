using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Providers;
using MeAjudaAi.Contracts.Shared;
using MeAjudaAi.Contracts.Modules.Users;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace MeAjudaAi.Modules.Communications.Application.Handlers;

/// <summary>
/// Consome o ProviderActivatedIntegrationEvent e enfileira e-mail de aprovação do prestador.
/// </summary>
public sealed class ProviderActivatedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogRepository logRepository,
    IUsersModuleApi usersModuleApi,
    ILogger<ProviderActivatedIntegrationEventHandler> logger)
    : IEventHandler<ProviderActivatedIntegrationEvent>
{
    private const string TemplateKey = "provider_activated";

    public async Task HandleAsync(
        ProviderActivatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"{TemplateKey}:{integrationEvent.ProviderId}";

        if (await logRepository.ExistsByCorrelationIdAsync(correlationId, cancellationToken))
        {
            logger.LogInformation(
                "Skipping provider activation email for {ProviderId} — already sent (correlationId: {CorrelationId}).",
                integrationEvent.ProviderId, correlationId);
            return;
        }

        var userResult = await usersModuleApi.GetUserByIdAsync(integrationEvent.UserId, cancellationToken);
        if (!userResult.IsSuccess || userResult.Value == null || string.IsNullOrWhiteSpace(userResult.Value.Email))
        {
            logger.LogWarning(
                "Could not resolve email for user {UserId}. Skipping provider activation email for provider {ProviderId}.",
                integrationEvent.UserId, integrationEvent.ProviderId);
            return;
        }

        var safeName = HtmlEncoder.Default.Encode(integrationEvent.Name);
        var recipientEmail = userResult.Value.Email;

        var payload = JsonSerializer.Serialize(new
        {
            To = recipientEmail,
            Subject = "Seu cadastro foi aprovado!",
            HtmlBody = $"<h1>Olá, {safeName}!</h1><p>Seu cadastro foi aprovado. Você já pode receber solicitações de serviço.</p>",
            TextBody = $"Olá, {integrationEvent.Name}!\nSeu cadastro foi aprovado. Você já pode receber solicitações de serviço.",
            From = (string?)null,
            CorrelationId = correlationId,
            TemplateKey = TemplateKey
        });

        var message = OutboxMessage.Create(
            channel: ECommunicationChannel.Email,
            payload: payload,
            priority: ECommunicationPriority.High,
            correlationId: correlationId);

        try
        {
            await outboxRepository.AddAsync(message, cancellationToken);
            await outboxRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Provider activation email enqueued for provider {ProviderId} (outboxId: {OutboxId}, correlationId: {CorrelationId}).",
                integrationEvent.ProviderId, message.Id, correlationId);
        }
        catch (Exception ex)
        {
            var processedException = MeAjudaAi.Shared.Database.Exceptions.PostgreSqlExceptionProcessor.ProcessException(
                ex as Microsoft.EntityFrameworkCore.DbUpdateException ?? new Microsoft.EntityFrameworkCore.DbUpdateException(ex.Message, ex));

            if (processedException is MeAjudaAi.Shared.Database.Exceptions.UniqueConstraintException)
            {
                logger.LogInformation(
                    "Skipping provider activation email for {ProviderId} — already enqueued or sent (correlationId: {CorrelationId}).",
                    integrationEvent.ProviderId, correlationId);
                return;
            }
            
            throw;
        }
    }
}
