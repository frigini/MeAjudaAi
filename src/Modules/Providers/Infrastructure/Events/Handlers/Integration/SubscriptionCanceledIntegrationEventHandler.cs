using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers.Integration;

/// <summary>
/// Handler para o evento de assinatura cancelada.
/// Rebaixa o tier do prestador para Standard.
/// </summary>
public sealed class SubscriptionCanceledIntegrationEventHandler(
    ProvidersDbContext dbContext,
    ILogger<SubscriptionCanceledIntegrationEventHandler> logger) : IEventHandler<SubscriptionCanceledIntegrationEvent>
{
    public async Task HandleAsync(SubscriptionCanceledIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Handling SubscriptionCanceledIntegrationEvent for user {UserId}", integrationEvent.UserId);

            var provider = await dbContext.Providers
                .FirstOrDefaultAsync(p => p.UserId == integrationEvent.UserId && !p.IsDeleted, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider not found for user {UserId} when handling SubscriptionCanceledIntegrationEvent.", integrationEvent.UserId);
                return;
            }
            
            provider.PromoteTier(EProviderTier.Standard, "payments-integration-canceled");
            await dbContext.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Demoted provider {ProviderId} to Standard tier.", provider.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling SubscriptionCanceledIntegrationEvent for user {UserId}", integrationEvent.UserId);
            throw;
        }
    }
}
