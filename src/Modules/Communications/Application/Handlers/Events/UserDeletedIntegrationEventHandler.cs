using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Database.Exceptions;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Application.Handlers.Events;

public sealed class UserDeletedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogQueries logQueries,
    [FromKeyedServices(SerializationKeys.Api)] ISerializer serializer,
    ILogger<UserDeletedIntegrationEventHandler> logger)
    : IEventHandler<UserDeletedIntegrationEvent>
{
    private const string TemplateKey = "user_deleted";

    public async Task HandleAsync(
        UserDeletedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"{TemplateKey}:{integrationEvent.UserId}";

        if (await logQueries.ExistsByCorrelationIdAsync(correlationId, cancellationToken))
        {
            logger.LogInformation(
                "Skipping user deleted email for user {UserId} — already sent (correlationId: {CorrelationId}).",
                integrationEvent.UserId, correlationId);
            return;
        }

        if (string.IsNullOrWhiteSpace(integrationEvent.Email) || string.IsNullOrWhiteSpace(integrationEvent.FirstName))
        {
            logger.LogWarning(
                "Skipping user deleted email for user {UserId} — missing required fields (Email: {HasEmail}, FirstName: {HasFirstName}).",
                integrationEvent.UserId,
                !string.IsNullOrWhiteSpace(integrationEvent.Email),
                !string.IsNullOrWhiteSpace(integrationEvent.FirstName));
            return;
        }

        var email = integrationEvent.Email;
        var firstName = integrationEvent.FirstName;

        var payload = serializer.Serialize(new
        {
            To = email,
            Subject = "Conta Excluída",
            HtmlBody = $"<h1>Olá, {firstName}!</h1><p>Sua conta no MeAjudaAi foi excluída com sucesso.</p>",
            TextBody = $"Olá, {firstName}!\nSua conta no MeAjudaAi foi excluída com sucesso.",
            TemplateKey = TemplateKey
        });

        var message = OutboxMessage.Create(
            channel: ECommunicationChannel.Email,
            payload: payload,
            maxRetries: 3,
            priority: ECommunicationPriority.Normal,
            correlationId: correlationId);

        try
        {
            await outboxRepository.AddAsync(message, cancellationToken);
            await outboxRepository.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "User deleted email enqueued for user {UserId} (outboxId: {OutboxId}, correlationId: {CorrelationId}).",
                integrationEvent.UserId, message.Id, correlationId);
        }
        catch (Exception ex)
        {
            var processedException = PostgreSqlExceptionProcessor.ProcessException(
                ex as DbUpdateException ?? new DbUpdateException(ex.Message, ex));

            if (processedException is UniqueConstraintException)
            {
                logger.LogInformation(
                    "Skipping user deleted email for user {UserId} — already enqueued (correlationId: {CorrelationId}).",
                    integrationEvent.UserId, correlationId);
                return;
            }

            logger.LogError(ex, "Failed to enqueue user deleted email for {UserId}.", integrationEvent.UserId);
            throw;
        }
    }
}
