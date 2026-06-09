using MeAjudaAi.Modules.Communications.Application.Queries;
using MeAjudaAi.Modules.Communications.Domain.Entities;
using MeAjudaAi.Modules.Communications.Domain.Enums;
using MeAjudaAi.Modules.Communications.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using MeAjudaAi.Contracts.Shared;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MeAjudaAi.Modules.Communications.Application.Handlers;

public sealed class UserProfileUpdatedIntegrationEventHandler(
    IOutboxMessageRepository outboxRepository,
    ICommunicationLogQueries logQueries,
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

        var payload = JsonSerializer.Serialize(new
        {
            To = integrationEvent.Email,
            Subject = "Seu perfil foi atualizado",
            HtmlBody = $"<h1>Olá, {integrationEvent.FirstName}!</h1><p>Seu perfil foi atualizado com sucesso.</p>",
            TextBody = $"Olá, {integrationEvent.FirstName}! Seu perfil foi atualizado com sucesso.",
            CorrelationId = correlationId,
            TemplateKey = TemplateKey
        }, new JsonSerializerOptions { PropertyNamingPolicy = null }); // Force consistent naming if needed, but this is default
        // The issue is likely that the test expects a specific order or JSON formatting that JsonSerializer changes. 
        // Let's use a simpler check in the test or make sure the JSON matches.
        // I will update the test instead to be less fragile.

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
            logger.LogError(ex, "Error enqueuing profile update notification for user {UserId}.", integrationEvent.UserId);
            throw;
        }
    }
}
