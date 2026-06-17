using MeAjudaAi.Modules.Payments.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Events.Handlers;

internal sealed class SubscriptionExpiredDomainEventHandler(
    IMessageBus messageBus,
    ILogger<SubscriptionExpiredDomainEventHandler> logger)
    : IEventHandler<SubscriptionExpiredDomainEvent>
{
    public async Task HandleAsync(SubscriptionExpiredDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            await messageBus.PublishAsync(
                new SubscriptionExpiredIntegrationEvent(
                    ModuleNames.Payments,
                    domainEvent.SubscriptionId,
                    domainEvent.ProviderId),
                null,
                cancellationToken);

            logger.LogInformation("SubscriptionExpiredIntegrationEvent published for subscription {SubscriptionId}", domainEvent.SubscriptionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing SubscriptionExpiredIntegrationEvent for subscription {SubscriptionId}", domainEvent.SubscriptionId);
            throw;
        }
    }
}
