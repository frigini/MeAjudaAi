using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.ServiceCatalogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers.Integration;

/// <summary>
/// Handler para processar eventos de atualização de nome de serviço do módulo ServiceCatalogs.
/// </summary>
public sealed class ServiceNameUpdatedIntegrationEventHandler(
    ProvidersDbContext dbContext,
    ILogger<ServiceNameUpdatedIntegrationEventHandler> logger) : IEventHandler<ServiceNameUpdatedIntegrationEvent>
{
    public async Task HandleAsync(ServiceNameUpdatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Handling ServiceNameUpdatedIntegrationEvent for service {ServiceId}, new name: {NewName}",
                integrationEvent.ServiceId,
                integrationEvent.NewName);

            var providersToUpdate = await dbContext.Providers
                .Include(p => p.Services)
                .Where(p => p.Services.Any(s => s.ServiceId == integrationEvent.ServiceId))
                .ToListAsync(cancellationToken);

            foreach (var provider in providersToUpdate)
            {
                var servicesToUpdate = provider.Services
                    .Where(s => s.ServiceId == integrationEvent.ServiceId);

                foreach (var service in servicesToUpdate)
                {
                    service.UpdateServiceName(integrationEvent.NewName);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Updated service name to {NewName} for {Count} providers.",
                integrationEvent.NewName,
                providersToUpdate.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error handling ServiceNameUpdatedIntegrationEvent for service {ServiceId}",
                integrationEvent.ServiceId);
            throw;
        }
    }
}
