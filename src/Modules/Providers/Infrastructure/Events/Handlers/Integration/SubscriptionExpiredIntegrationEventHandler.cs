using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database.Idempotency;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers.Integration;

/// <summary>
/// Handler para o evento de assinatura expirada.
/// Rebaixa o tier do prestador para Standard.
/// </summary>
public sealed class SubscriptionExpiredIntegrationEventHandler(
    ProvidersDbContext dbContext,
    IIdempotencyRepository idempotencyRepository,
    ILogger<SubscriptionExpiredIntegrationEventHandler> logger) : IEventHandler<SubscriptionExpiredIntegrationEvent>
{
    public async Task HandleAsync(SubscriptionExpiredIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var correlationId = integrationEvent.SubscriptionId.ToString();
            logger.LogInformation("Handling SubscriptionExpiredIntegrationEvent for user {UserId}", integrationEvent.UserId);

            // Verificar idempotência
            if (await idempotencyRepository.IsProcessedAsync(correlationId, cancellationToken))
            {
                logger.LogInformation("Event {CorrelationId} already processed.", correlationId);
                return;
            }

            var provider = await dbContext.Providers
                .FirstOrDefaultAsync(p => p.UserId == integrationEvent.UserId && !p.IsDeleted, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider not found for user {UserId} when handling SubscriptionExpiredIntegrationEvent.", integrationEvent.UserId);
                return;
            }
            
            provider.DemoteTier(EProviderTier.Standard, "payments-integration-expired");
            
            // Registrar processamento
            await idempotencyRepository.MarkAsProcessedAsync(correlationId, cancellationToken);
            
            await dbContext.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Demoted provider {ProviderId} to Standard tier.", provider.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling SubscriptionExpiredIntegrationEvent for user {UserId}", integrationEvent.UserId);
            throw;
        }
    }
}





