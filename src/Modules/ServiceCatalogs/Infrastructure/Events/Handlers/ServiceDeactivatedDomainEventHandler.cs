using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Events.Handlers;

public sealed class ServiceDeactivatedDomainEventHandler(
    IMessageBus messageBus,
    ILogger<ServiceDeactivatedDomainEventHandler> logger) : IEventHandler<ServiceDeactivatedDomainEvent>
{
    public async Task HandleAsync(ServiceDeactivatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var integrationEvent = new ServiceDeactivatedIntegrationEvent(
                ModuleNames.ServiceCatalogs,
                domainEvent.ServiceId.Value);

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            
            logger.LogInformation("Published ServiceDeactivatedIntegrationEvent for service {ServiceId}", domainEvent.ServiceId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ServiceDeactivatedDomainEvent for service {ServiceId}", domainEvent.ServiceId);
            throw;
        }
    }
}
