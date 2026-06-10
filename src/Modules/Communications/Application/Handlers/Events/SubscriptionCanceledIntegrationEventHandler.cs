using MeAjudaAi.Contracts.Enums;
using MeAjudaAi.Contracts.Modules.Users;
using MeAjudaAi.Modules.Communications.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using MeAjudaAi.Shared.Serialization;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Application.Handlers.Events;

public sealed class SubscriptionCanceledIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogQueries logQueries,
    IUsersModuleApi usersModuleApi,
    [FromKeyedServices(SerializationKeys.Api)] ISerializer serializer,
    ILogger<SubscriptionCanceledIntegrationEventHandler> logger)
    : IEventHandler<SubscriptionCanceledIntegrationEvent>
{
    private const string TemplateKey = "subscription_canceled";

    public async Task HandleAsync(
        SubscriptionCanceledIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"{TemplateKey}:{integrationEvent.SubscriptionId}";

        if (await logQueries.ExistsByCorrelationIdAsync(correlationId, cancellationToken))
        {
            logger.LogInformation(
                "Skipping subscription canceled email for user {UserId} — already sent (correlationId: {CorrelationId}).",
                integrationEvent.UserId, correlationId);
            return;
        }

        var userResult = await usersModuleApi.GetUserByIdAsync(integrationEvent.UserId, cancellationToken);
        if (!userResult.IsSuccess)
        {
            logger.LogError("Failed to get user {UserId} for subscription {SubscriptionId}.", integrationEvent.UserId, integrationEvent.SubscriptionId);
            return;
        }

        var payload = serializer.Serialize(new
        {
            To = userResult.Value!.Email,
            Subject = "Assinatura Cancelada",
            HtmlBody = $"<h1>Olá, {userResult.Value.FirstName}!</h1><p>Sua assinatura foi cancelada.</p>",
            TextBody = $"Olá, {userResult.Value.FirstName}!\nSua assinatura foi cancelada.",
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
                "Subscription canceled email enqueued for user {UserId} (outboxId: {OutboxId}, correlationId: {CorrelationId}).",
                integrationEvent.UserId, message.Id, correlationId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enqueue subscription canceled email for {UserId}.", integrationEvent.UserId);
            throw;
        }
    }
}
