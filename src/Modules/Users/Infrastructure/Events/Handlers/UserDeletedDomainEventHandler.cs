using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Modules.Users.Infrastructure.Mappers;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Infrastructure.Events.Handlers;

/// <summary>
/// Manipula UserDeletedDomainEvent e publica UserDeletedIntegrationEvent
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

            // Cria evento de integração para notificar outros módulos
            var integrationEvent = domainEvent.ToIntegrationEvent();

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
