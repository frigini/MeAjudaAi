using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Infrastructure.Mappers;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Infrastructure.Events.Handlers;

/// <summary>
/// Manipula UserDeletedDomainEvent e publica UserDeletedIntegrationEvent
/// </summary>
internal sealed class UserDeletedDomainEventHandler(
    IMessageBus messageBus,
    UsersDbContext context,
    ILogger<UserDeletedDomainEventHandler> logger) : IEventHandler<UserDeletedDomainEvent>
{
    public async Task HandleAsync(UserDeletedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Handling UserDeletedDomainEvent for user {UserId}", domainEvent.AggregateId);

            var userData = await context.Users
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(u => u.Id.Value == domainEvent.AggregateId)
                .Select(u => new { u.Email.Value, u.FirstName })
                .FirstOrDefaultAsync(cancellationToken);

            var integrationEvent = domainEvent.ToIntegrationEvent(
                email: userData?.Value ?? "desconhecido",
                firstName: userData?.FirstName ?? "Usuário");

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully published UserDeleted integration event for user {UserId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling UserDeletedDomainEvent for user {UserId}", domainEvent.AggregateId);
            throw new InvalidOperationException(
                $"Failed to publish UserDeleted integration event for user '{domainEvent.AggregateId}'",
                ex);
        }
    }
}
