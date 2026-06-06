using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers.Integration;

/// <summary>
/// Handler para processar eventos de ativação de assinatura vindos do módulo de pagamentos.
/// Promove o tier do prestador no módulo de Providers.
/// </summary>
public sealed class SubscriptionActivatedIntegrationEventHandler(
    IUnitOfWork unitOfWork,
    ILogger<SubscriptionActivatedIntegrationEventHandler> logger) : IEventHandler<SubscriptionActivatedIntegrationEvent>
{
    public async Task HandleAsync(SubscriptionActivatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Handling SubscriptionActivatedIntegrationEvent for provider {ProviderId}, subscription {SubscriptionId}",
                integrationEvent.UserId,
                integrationEvent.SubscriptionId);

            // No evento, UserId contém o ProviderId
            var providerId = new ProviderId(integrationEvent.UserId);
            var repository = unitOfWork.GetRepository<Provider, ProviderId>();
            var provider = await repository.TryFindAsync(providerId, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found when handling SubscriptionActivatedIntegrationEvent.", integrationEvent.UserId);
                return;
            }

            // NOTA: Como o evento não traz o PlanId, teríamos que buscar no módulo Payments.
            // Para simplificar esta auditoria/implementação inicial, vamos assumir que o Tier deve ser promovido.
            // Em uma implementação real, o evento deveria conter o PlanId ou chamaríamos a IPaymentsModuleApi.
            
            // Por enquanto, vamos apenas logar e marcar como Premium/Gold se o tier atual for Standard.
            if (provider.Tier == EProviderTier.Standard)
            {
                logger.LogInformation("Promoting provider {ProviderId} to Gold tier (placeholder logic).", provider.Id);
                provider.PromoteTier(EProviderTier.Gold, "payments-integration");
                
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling SubscriptionActivatedIntegrationEvent for provider {ProviderId}", integrationEvent.UserId);
            throw;
        }
    }
}
