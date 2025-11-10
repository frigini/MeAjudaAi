using MeAjudaAi.Modules.Providers.Domain.Events;
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
public sealed class ProviderDeletedDomainEventHandler(
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

            // Buscar apenas o UserId do provider, mesmo se soft-deleted ou removido
            var userId = await context.Providers
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(p => p.Id.Value == domainEvent.AggregateId)
                .Select(p => (Guid?)p.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (userId.HasValue)
            {
                var integrationEvent = domainEvent.ToIntegrationEvent(userId.Value);
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
            throw;
        }
    }
}