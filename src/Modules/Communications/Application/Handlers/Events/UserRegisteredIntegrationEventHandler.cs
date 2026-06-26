using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Application.Handlers;

public sealed class UserRegisteredIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogQueries logQueries,
    [FromKeyedServices(SerializationKeys.Api)] ISerializer serializer,
    ILogger<UserRegisteredIntegrationEventHandler> logger)
    : IEventHandler<UserRegisteredIntegrationEvent>
{
    private const string TemplateKey = CommunicationTemplateKeys.UserRegistered;

    public async Task HandleAsync(
        UserRegisteredIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"{TemplateKey}:{integrationEvent.UserId}";

        if (await logQueries.ExistsByCorrelationIdAsync(correlationId, cancellationToken))
        {
            logger.LogInformation(
                "Skipping welcome email for user {UserId} — already sent (correlationId: {CorrelationId}).",
                integrationEvent.UserId, correlationId);
            return;
        }

        var payload = serializer.Serialize(new
        {
            To = integrationEvent.Email,
            Subject = "Bem-vindo ao MeAjudaAi!",
            HtmlBody = $"<h1>Olá, {integrationEvent.FirstName}!</h1><p>Seja bem-vindo(a) ao MeAjudaAi.</p>",
            TextBody = $"Olá, {integrationEvent.FirstName}!\nSeja bem-vindo(a) ao MeAjudaAi.",
            From = (string?)null,
            CorrelationId = correlationId
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
                "Welcome email enqueued for user {UserId} (outboxId: {OutboxId}, correlationId: {CorrelationId}).",
                integrationEvent.UserId, message.Id, correlationId);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            var processedException = MeAjudaAi.Shared.Database.Exceptions.PostgreSqlExceptionProcessor.ProcessException(ex);

            if (processedException is MeAjudaAi.Shared.Database.Exceptions.UniqueConstraintException)
            {
                logger.LogInformation(
                    "Skipping welcome email for user {UserId} — already enqueued or sent (correlationId: {CorrelationId}).",
                    integrationEvent.UserId, correlationId);
                return;
            }
            
            throw;
        }
    }
}
