using MeAjudaAi.Modules.Users.Domain.Entities;
using MeAjudaAi.Modules.Users.Infrastructure.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Infrastructure.Messaging
{
    public class UserEventPublisher(IMessageBus messageBus, ILogger<UserEventPublisher> logger) : IUserEventPublisher
    {
        public async Task PublishUserRegisteredAsync(User user, CancellationToken cancellationToken = default)
        {
            var integrationEvent = new UserRegisteredIntegrationEvent(
                user.Id.Value,
                user.Email.Value,
                user.Profile.FirstName,
                user.Profile.LastName,
                user.Roles.FirstOrDefault() ?? "Customer",
                user.CreatedAt
            );

            try
            {
                await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
                logger.LogInformation("Published UserRegistered event for user {UserId}", user.Id.Value);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish UserRegistered event for user {UserId}", user.Id.Value);
                throw;
            }
        }

        public async Task PublishUserRoleChangedAsync(User user, string previousRole, string newRole, CancellationToken cancellationToken = default)
        {
            var integrationEvent = new UserRoleChangedIntegrationEvent(
                user.Id.Value,
                previousRole,
                newRole,
                "System",
                DateTime.UtcNow
            );

            try
            {
                await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
                logger.LogInformation("Published UserRoleChanged event for user {UserId}", user.Id.Value);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish UserRoleChanged event for user {UserId}", user.Id.Value);
                throw;
            }
        }

        public async Task PublishUserLockedOutAsync(User user, string reason, CancellationToken cancellationToken = default)
        {
            var integrationEvent = new UserLockedOutIntegrationEvent(
                user.Id.Value,
                user.Email.Value,
                reason,
                DateTime.UtcNow,
                null
            );

            try
            {
                await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
                logger.LogInformation("Published UserLockedOut event for user {UserId}", user.Id.Value);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish UserLockedOut event for user {UserId}", user.Id.Value);
                throw;
            }
        }
    }
}