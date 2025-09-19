using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Infrastructure.Events.Handlers;

/// <summary>
/// Handles UserDeletedDomainEvent and publishes UserDeletedIntegrationEvent
/// </summary>
internal sealed class UserDeletedDomainEventHandler(
    IMessageBus messageBus,
    ILogger<UserDeletedDomainEventHandler> logger) : IEventHandler<UserDeletedDomainEvent>
{
    public async Task HandleAsync(UserDeletedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Handling UserDeletedDomainEvent for user {UserId}", domainEvent.AggregateId);

            // Create integration event to notify other modules
            var integrationEvent = new UserDeletedIntegrationEvent(
                Source: "Users",
                UserId: domainEvent.AggregateId,
                DeletedAt: DateTime.UtcNow
            );

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully published UserDeleted integration event for user {UserId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling UserDeletedDomainEvent for user {UserId}", domainEvent.AggregateId);
            throw;
        }
    }
}