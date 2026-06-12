using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database.Idempotency;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers.Integration;

/// <summary>
/// Handler para processar eventos de ativação de assinatura vindos do módulo de pagamentos.
/// Promove o tier do prestador no módulo de Providers.
/// </summary>
public sealed class SubscriptionActivatedIntegrationEventHandler(
    ProvidersDbContext dbContext,
    IIdempotencyRepository idempotencyRepository,
    ILogger<SubscriptionActivatedIntegrationEventHandler> logger) : IEventHandler<SubscriptionActivatedIntegrationEvent>
{
    public async Task HandleAsync(SubscriptionActivatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var correlationId = integrationEvent.SubscriptionId.ToString();
            logger.LogInformation(
                "Handling SubscriptionActivatedIntegrationEvent for user {UserId}, subscription {SubscriptionId}",
                integrationEvent.UserId,
                integrationEvent.SubscriptionId);

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
                logger.LogWarning("Provider not found for user {UserId} when handling SubscriptionActivatedIntegrationEvent.", integrationEvent.UserId);
                return;
            }
            
            provider.PromoteTier(EProviderTier.Gold, "payments-integration-activated");
            
            // Registrar processamento
            await idempotencyRepository.MarkAsProcessedAsync(correlationId, cancellationToken);
            
            await dbContext.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Promoted provider {ProviderId} to Gold tier.", provider.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling SubscriptionActivatedIntegrationEvent for user {UserId}", integrationEvent.UserId);
            throw;
        }
    }
}





