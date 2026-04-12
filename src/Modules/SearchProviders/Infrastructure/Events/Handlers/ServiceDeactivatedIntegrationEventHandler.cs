using MeAjudaAi.Modules.SearchProviders.Domain.Repositories;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;

/// <summary>
/// Handler para remover um serviço de todos os prestadores pesquisáveis quando ele é desativado no catálogo.
/// </summary>
public sealed class ServiceDeactivatedIntegrationEventHandler(
    ISearchableProviderRepository repository,
    ILogger<ServiceDeactivatedIntegrationEventHandler> logger) : IEventHandler<ServiceDeactivatedIntegrationEvent>
{
    public async Task HandleAsync(ServiceDeactivatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Handling ServiceDeactivatedIntegrationEvent for service {ServiceId}", integrationEvent.ServiceId);

            var providers = await repository.GetByServiceIdAsync(integrationEvent.ServiceId, cancellationToken);
            
            if (!providers.Any())
            {
                return;
            }

            foreach (var provider in providers)
            {
                var updatedServices = provider.ServiceIds.Where(id => id != integrationEvent.ServiceId).ToArray();
                provider.UpdateServices(updatedServices);
                await repository.UpdateAsync(provider, cancellationToken);
            }

            await repository.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Successfully removed service {ServiceId} from {Count} searchable providers.", 
                integrationEvent.ServiceId, providers.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ServiceDeactivatedIntegrationEvent for service {ServiceId}", integrationEvent.ServiceId);
        }
    }
}
