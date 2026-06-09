using MeAjudaAi.Modules.Payments.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.Payments;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Payments.Infrastructure.Events.Handlers;

public sealed class SubscriptionCanceledDomainEventHandler(
    IMessageBus messageBus,
    ILogger<SubscriptionCanceledDomainEventHandler> logger)
    : IEventHandler<SubscriptionCanceledDomainEvent>
{
    public async Task HandleAsync(SubscriptionCanceledDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            await messageBus.PublishAsync(
                new SubscriptionCanceledIntegrationEvent(
                    ModuleNames.Payments,
                    domainEvent.SubscriptionId,
                    domainEvent.ProviderId),
                null,
                cancellationToken);

            logger.LogInformation("SubscriptionCanceledIntegrationEvent published for subscription {SubscriptionId}", domainEvent.SubscriptionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing SubscriptionCanceledIntegrationEvent for subscription {SubscriptionId}", domainEvent.SubscriptionId);
            throw;
        }
    }
}
