using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Events.Handlers;

public sealed class ServiceActivatedDomainEventHandler(
    IServiceRepository serviceRepository,
    IMessageBus messageBus,
    ILogger<ServiceActivatedDomainEventHandler> logger) : IEventHandler<ServiceActivatedDomainEvent>
{
    public async Task HandleAsync(ServiceActivatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = await serviceRepository.GetByIdAsync(domainEvent.ServiceId, cancellationToken);
            if (service == null)
            {
                logger.LogWarning("Service {ServiceId} not found when handling activation event.", domainEvent.ServiceId);
                return;
            }

            var integrationEvent = new ServiceActivatedIntegrationEvent(
                "ServiceCatalogs",
                service.Id.Value,
                service.Name);

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            
            logger.LogInformation("Published ServiceActivatedIntegrationEvent for service {ServiceId}", service.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ServiceActivatedDomainEvent for service {ServiceId}", domainEvent.ServiceId);
        }
    }
}
