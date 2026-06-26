using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Mappers;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;

/// <summary>
/// Manipula eventos de domínio ProviderDeletedDomainEvent e publica eventos de integração.
/// </summary>
internal sealed class ProviderDeletedDomainEventHandler(
    IMessageBus messageBus,
    ProvidersDbContext context,
    ILogger<ProviderDeletedDomainEventHandler> logger) : IEventHandler<ProviderDeletedDomainEvent>
{
    /// <summary>
    /// Processa o evento de domínio de prestador excluído de forma assíncrona.
    /// </summary>
    public async Task HandleAsync(ProviderDeletedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Handling ProviderDeletedDomainEvent for provider {ProviderId}", domainEvent.AggregateId);

            var provider = await context.Providers
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == new ProviderId(domainEvent.AggregateId), cancellationToken);

            if (provider is not null)
            {
                var integrationEvent = domainEvent.ToIntegrationEvent(
                    userId: provider.UserId,
                    email: provider.BusinessProfile?.ContactInfo?.Email ?? "unknown");
                await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);

                logger.LogInformation("Successfully published ProviderDeleted integration event for provider {ProviderId}", domainEvent.AggregateId);
            }
            else
            {
                logger.LogWarning("Provider {ProviderId} not found when handling ProviderDeletedDomainEvent", domainEvent.AggregateId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ProviderDeletedDomainEvent for provider {ProviderId}", domainEvent.AggregateId);
            throw new InvalidOperationException(
                $"Failed to publish ProviderDeleted integration event for provider '{domainEvent.AggregateId}'",
                ex);
        }
    }
}
