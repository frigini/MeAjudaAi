using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Events.Handlers;

public sealed class SubscriptionRenewedIntegrationEventHandler(
    IUnitOfWork unitOfWork,
    ILogger<SubscriptionRenewedIntegrationEventHandler> logger) : IEventHandler<SubscriptionRenewedIntegrationEvent>
{
    public async Task HandleAsync(SubscriptionRenewedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling SubscriptionRenewed for subscription {SubscriptionId}", integrationEvent.SubscriptionId);

        var repository = unitOfWork.GetRepository<Domain.Entities.Subscription, Guid>();
        var subscription = await repository.TryFindAsync(integrationEvent.SubscriptionId, cancellationToken);

        if (subscription == null)
        {
            logger.LogWarning("Subscription {SubscriptionId} not found.", integrationEvent.SubscriptionId);
            return;
        }

        // Renova a assinatura (usando uma data futura hipotética já que o evento não traz a nova expiração)
        // O método Renew valida se o estado é Active e atualiza a data.
        subscription.Renew(DateTime.UtcNow.AddMonths(1));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Subscription {SubscriptionId} renewed successfully.", integrationEvent.SubscriptionId);
    }
}
