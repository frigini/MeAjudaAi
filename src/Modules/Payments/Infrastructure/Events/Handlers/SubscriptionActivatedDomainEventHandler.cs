using MeAjudaAi.Modules.Payments.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Events.Handlers;

public sealed class SubscriptionActivatedDomainEventHandler(
    IMessageBus messageBus,
    ILogger<SubscriptionActivatedDomainEventHandler> logger)
    : IEventHandler<SubscriptionActivatedDomainEvent>
{
    public async Task HandleAsync(SubscriptionActivatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            await messageBus.PublishAsync(
                new SubscriptionActivatedIntegrationEvent(
                    ModuleNames.Payments,
                    domainEvent.SubscriptionId,
                    domainEvent.ProviderId),
                null,
                cancellationToken);

            logger.LogInformation("SubscriptionActivatedIntegrationEvent published for subscription {SubscriptionId}", domainEvent.SubscriptionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing SubscriptionActivatedIntegrationEvent for subscription {SubscriptionId}", domainEvent.SubscriptionId);
            throw;
        }
    }
}
