using MeAjudaAi.Modules.Payments.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Events.Handlers;

internal sealed class SubscriptionRenewedDomainEventHandler(
    IMessageBus messageBus,
    ILogger<SubscriptionRenewedDomainEventHandler> logger)
    : IEventHandler<SubscriptionRenewedDomainEvent>
{
    public async Task HandleAsync(SubscriptionRenewedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            await messageBus.PublishAsync(
                new SubscriptionRenewedIntegrationEvent(
                    ModuleNames.Payments,
                    domainEvent.SubscriptionId,
                    domainEvent.ProviderId,
                    domainEvent.NewExpiresAt),
                null,
                cancellationToken);

            logger.LogInformation("SubscriptionRenewedIntegrationEvent published for subscription {SubscriptionId}", domainEvent.SubscriptionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing SubscriptionRenewedIntegrationEvent for subscription {SubscriptionId}", domainEvent.SubscriptionId);
            throw;
        }
    }
}
