using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Events.Handlers;

/// <summary>
/// Manipulador de eventos de domínio para o evento ServiceActivatedDomainEvent.
/// </summary>
/// <param name="serviceQueries"></param>
/// <param name="messageBus"></param>
/// <param name="logger"></param>
internal sealed class ServiceActivatedDomainEventHandler(
    IServiceQueries serviceQueries,
    IMessageBus messageBus,
    ILogger<ServiceActivatedDomainEventHandler> logger) : IEventHandler<ServiceActivatedDomainEvent>
{
    public async Task HandleAsync(ServiceActivatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = await serviceQueries.GetByIdAsync(domainEvent.ServiceId, cancellationToken)??throw new InvalidOperationException($"Service {domainEvent.ServiceId} not found when handling activation event.");
            var integrationEvent = new ServiceActivatedIntegrationEvent(
                ModuleNames.ServiceCatalogs,
                service.Id.Value,
                service.Name);

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            
            logger.LogInformation("Published ServiceActivatedIntegrationEvent for service {ServiceId}", service.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ServiceActivatedDomainEvent for service {ServiceId}", domainEvent.ServiceId);
            throw;
        }
    }
}
