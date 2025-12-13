using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Infrastructure.Mappers;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Infrastructure.Events.Handlers;

/// <summary>
/// Manipula UserProfileUpdatedDomainEvent e publica UserProfileUpdatedIntegrationEvent
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

            // Busca o usuário com informações atualizadas do perfil
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == new Domain.ValueObjects.UserId(domainEvent.AggregateId), cancellationToken);

            if (user == null)
            {
                logger.LogWarning("User {UserId} not found when handling UserProfileUpdatedDomainEvent", domainEvent.AggregateId);
                return;
            }

            // Cria evento de integração usando mapper
            var integrationEvent = domainEvent.ToIntegrationEvent(user.Email.Value);

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully published UserProfileUpdated integration event for user {UserId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling UserProfileUpdatedDomainEvent for user {UserId}", domainEvent.AggregateId);
            throw new InvalidOperationException(
                $"Error handling UserProfileUpdatedDomainEvent for user '{domainEvent.AggregateId}'",
                ex);
        }
    }
}
