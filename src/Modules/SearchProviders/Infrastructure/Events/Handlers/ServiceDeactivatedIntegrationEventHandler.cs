using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Modules.SearchProviders.Application.Queries;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;

/// <summary>
/// Handler para remover um serviço de todos os prestadores pesquisáveis quando ele é desativado no catálogo.
/// </summary>
public sealed class ServiceDeactivatedIntegrationEventHandler(
    [FromKeyedServices(ModuleKeys.SearchProviders)] IUnitOfWork uow,
    ISearchableProviderQueries queries,
    ILogger<ServiceDeactivatedIntegrationEventHandler> logger) : IEventHandler<ServiceDeactivatedIntegrationEvent>
{
    public async Task HandleAsync(ServiceDeactivatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling ServiceDeactivatedIntegrationEvent for service {ServiceId}", integrationEvent.ServiceId);

        var providers = await queries.GetByServiceIdAsync(integrationEvent.ServiceId, track: true, cancellationToken);
        
        if (!providers.Any())
        {
            return;
        }

        foreach (var provider in providers)
        {
            var updatedServices = provider.ServiceIds.Where(id => id != integrationEvent.ServiceId).ToArray();
            provider.UpdateServices(updatedServices);
        }

        await uow.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Successfully removed service {ServiceId} from {Count} searchable providers.", 
            integrationEvent.ServiceId, providers.Count);
    }
}



