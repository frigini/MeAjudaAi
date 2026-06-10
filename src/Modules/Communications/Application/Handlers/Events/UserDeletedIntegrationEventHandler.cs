using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Contracts.Modules.Users;
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

namespace MeAjudaAi.Modules.Communications.Application.Handlers.Events;

public sealed class UserDeletedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogQueries logQueries,
    IUsersModuleApi usersModuleApi,
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

        var userResult = await usersModuleApi.GetUserByIdAsync(integrationEvent.UserId, cancellationToken);
        if (!userResult.IsSuccess)
        {
            logger.LogError("Failed to get user {UserId} for deletion notification.", integrationEvent.UserId);
            return;
        }

        var payload = serializer.Serialize(new
        {
            To = userResult.Value!.Email,
            Subject = "Conta Excluída",
            HtmlBody = $"<h1>Olá, {userResult.Value.FirstName}!</h1><p>Sua conta no MeAjudaAi foi excluída com sucesso.</p>",
            TextBody = $"Olá, {userResult.Value.FirstName}!\nSua conta no MeAjudaAi foi excluída com sucesso.",
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
            logger.LogError(ex, "Failed to enqueue user deleted email for {UserId}.", integrationEvent.UserId);
            throw;
        }
    }
}
