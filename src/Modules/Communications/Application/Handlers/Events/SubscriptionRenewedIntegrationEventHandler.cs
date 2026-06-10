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

public sealed class SubscriptionRenewedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogQueries logQueries,
    IUsersModuleApi usersModuleApi,
    [FromKeyedServices(SerializationKeys.Api)] ISerializer serializer,
    ILogger<SubscriptionRenewedIntegrationEventHandler> logger)
    : IEventHandler<SubscriptionRenewedIntegrationEvent>
{
    private const string TemplateKey = "subscription_renewed";

    public async Task HandleAsync(
        SubscriptionRenewedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var correlationId = $"{TemplateKey}:{integrationEvent.SubscriptionId}";

        if (await logQueries.ExistsByCorrelationIdAsync(correlationId, cancellationToken))
        {
            logger.LogInformation(
                "Skipping subscription renewed email for user {UserId} — already sent (correlationId: {CorrelationId}).",
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
            Subject = "Assinatura Renovada",
            HtmlBody = $"<h1>Olá, {userResult.Value.FirstName}!</h1><p>Sua assinatura foi renovada.</p>",
            TextBody = $"Olá, {userResult.Value.FirstName}!\nSua assinatura foi renovada.",
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
                "Subscription renewed email enqueued for user {UserId} (outboxId: {OutboxId}, correlationId: {CorrelationId}).",
                integrationEvent.UserId, message.Id, correlationId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enqueue subscription renewed email for {UserId}.", integrationEvent.UserId);
            throw;
        }
    }
}
