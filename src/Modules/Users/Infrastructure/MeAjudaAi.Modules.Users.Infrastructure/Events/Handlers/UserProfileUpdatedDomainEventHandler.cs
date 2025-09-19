using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Infrastructure.Events.Handlers;

/// <summary>
/// Handles UserProfileUpdatedDomainEvent and publishes UserProfileUpdatedIntegrationEvent
/// </summary>
internal sealed class UserProfileUpdatedDomainEventHandler(
    IMessageBus messageBus,
    UsersDbContext context,
    ILogger<UserProfileUpdatedDomainEventHandler> logger) : IEventHandler<UserProfileUpdatedDomainEvent>
{
    public async Task HandleAsync(UserProfileUpdatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Handling UserProfileUpdatedDomainEvent for user {UserId}", domainEvent.AggregateId);

            // Get the user with updated profile information
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == new Domain.ValueObjects.UserId(domainEvent.AggregateId), cancellationToken);

            if (user == null)
            {
                logger.LogWarning("User {UserId} not found when handling UserProfileUpdatedDomainEvent", domainEvent.AggregateId);
                return;
            }

            // Create integration event
            var integrationEvent = new UserProfileUpdatedIntegrationEvent(
                Source: "Users",
                UserId: domainEvent.AggregateId,
                Email: user.Email.Value,
                FirstName: domainEvent.FirstName,
                LastName: domainEvent.LastName,
                UpdatedAt: DateTime.UtcNow
            );

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully published UserProfileUpdated integration event for user {UserId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling UserProfileUpdatedDomainEvent for user {UserId}", domainEvent.AggregateId);
            throw;
        }
    }
}
