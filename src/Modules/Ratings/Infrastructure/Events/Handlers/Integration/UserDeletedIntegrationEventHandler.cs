using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database.Idempotency;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Ratings.Infrastructure.Events.Handlers.Integration;

/// <summary>
/// Handler para o evento de usuário excluído.
/// Remove as avaliações feitas pelo usuário.
/// </summary>
public sealed class UserDeletedIntegrationEventHandler(
    RatingsDbContext dbContext,
    IIdempotencyRepository idempotencyRepository,
    ILogger<UserDeletedIntegrationEventHandler> logger)
    : IEventHandler<UserDeletedIntegrationEvent>
{
    public async Task HandleAsync(
        UserDeletedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var correlationId = integrationEvent.Id.ToString(); // Using the Event ID as correlation ID
            logger.LogInformation("Handling UserDeletedIntegrationEvent for user {UserId}", integrationEvent.UserId);

            // Verificar idempotência
            if (await idempotencyRepository.IsProcessedAsync(correlationId, cancellationToken))
            {
                logger.LogInformation("Event {CorrelationId} already processed.", correlationId);
                return;
            }

            var reviewsToRemove = await dbContext.Reviews
                .Where(r => r.CustomerId == integrationEvent.UserId)
                .ToListAsync(cancellationToken);

            if (reviewsToRemove.Count > 0)
            {
                dbContext.Reviews.RemoveRange(reviewsToRemove);
            }

            // Registrar processamento
            await idempotencyRepository.MarkAsProcessedAsync(correlationId, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Successfully removed {Count} reviews for deleted user {UserId} and recorded event.", reviewsToRemove.Count, integrationEvent.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing reviews for deleted user {UserId}", integrationEvent.UserId);
            throw;
        }
    }
}
