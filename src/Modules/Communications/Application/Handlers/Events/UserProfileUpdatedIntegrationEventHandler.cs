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

namespace MeAjudaAi.Modules.Communications.Application.Handlers;

public sealed class UserProfileUpdatedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogQueries logQueries,
    [FromKeyedServices(SerializationKeys.Api)] ISerializer serializer,
    ILogger<UserProfileUpdatedIntegrationEventHandler> logger)
    : IEventHandler<UserProfileUpdatedIntegrationEvent>
{
    private const string TemplateKey = "user_profile_updated";

    public async Task HandleAsync(
        UserProfileUpdatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"{TemplateKey}:{integrationEvent.UserId}:{integrationEvent.OccurredAt.Ticks}";

        if (await logQueries.ExistsByCorrelationIdAsync(correlationId, cancellationToken))
        {
            return;
        }

        var payload = serializer.Serialize(new
        {
            To = integrationEvent.Email,
            Subject = "Seu perfil foi atualizado",
            HtmlBody = $"<h1>Olá, {integrationEvent.FirstName}!</h1><p>Seu perfil foi atualizado com sucesso.</p>",
            TextBody = $"Olá, {integrationEvent.FirstName}! Seu perfil foi atualizado com sucesso.",
            CorrelationId = correlationId,
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

            logger.LogInformation("Profile update notification enqueued for user {UserId}.", integrationEvent.UserId);
        }
        catch (Exception ex)
        {
            var processedException = PostgreSqlExceptionProcessor.ProcessException(
                ex as DbUpdateException ?? new DbUpdateException(ex.Message, ex));

            if (processedException is UniqueConstraintException)
            {
                logger.LogInformation(
                    "Skipping profile update notification for user {UserId} — already enqueued (correlationId: {CorrelationId}).",
                    integrationEvent.UserId, correlationId);
                return;
            }

            logger.LogError(ex, "Error enqueuing profile update notification for user {UserId}.", integrationEvent.UserId);
            throw;
        }
    }
}
