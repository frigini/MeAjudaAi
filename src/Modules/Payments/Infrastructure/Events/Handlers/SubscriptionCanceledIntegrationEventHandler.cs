using MeAjudaAi.Modules.Payments.Domain.Abstractions;
using MeAjudaAi.Modules.Payments.Domain.Enums;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Events.Handlers;

public sealed class SubscriptionCanceledIntegrationEventHandler(
    IUnitOfWork unitOfWork,
    ILogger<SubscriptionCanceledIntegrationEventHandler> logger) : IEventHandler<SubscriptionCanceledIntegrationEvent>
{
    public async Task HandleAsync(SubscriptionCanceledIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling SubscriptionCanceled for subscription {SubscriptionId}", integrationEvent.SubscriptionId);

        var repository = unitOfWork.GetRepository<Domain.Entities.Subscription, Guid>();
        var subscription = await repository.TryFindAsync(integrationEvent.SubscriptionId, cancellationToken);

        if (subscription == null)
        {
            logger.LogWarning("Subscription {SubscriptionId} not found.", integrationEvent.SubscriptionId);
            return;
        }

        subscription.SetStatus(ESubscriptionStatus.Canceled);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Subscription {SubscriptionId} updated to Canceled.", integrationEvent.SubscriptionId);
    }
}
