using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Events.Service;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Events.Handlers;

/// <summary>
/// Manipulador de eventos de domínio para o evento ServiceUpdatedDomainEvent.
/// </summary>
/// <param name="serviceQueries"></param>
/// <param name="messageBus"></param>
/// <param name="logger"></param>
internal sealed class ServiceUpdatedDomainEventHandler(
    IServiceQueries serviceQueries,
    IMessageBus messageBus,
    ILogger<ServiceUpdatedDomainEventHandler> logger) : IEventHandler<ServiceUpdatedDomainEvent>
{
    public async Task HandleAsync(ServiceUpdatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = await serviceQueries.GetByIdAsync(domainEvent.ServiceId.Value, cancellationToken)??throw new InvalidOperationException($"Service {domainEvent.ServiceId.Value} not found when handling update event.");
            var integrationEvent = new ServiceNameUpdatedIntegrationEvent(
                ModuleNames.ServiceCatalogs,
                service.Id,
                service.Name);

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);
            
            logger.LogInformation("Published ServiceNameUpdatedIntegrationEvent for service {ServiceId}", service.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ServiceUpdatedDomainEvent for service {ServiceId}", domainEvent.ServiceId.Value);
            throw;
        }
    }
}
