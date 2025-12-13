using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Mappers;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;

/// <summary>
/// Manipula eventos de domínio ProviderProfileUpdatedDomainEvent e publica eventos de integração.
/// </summary>
public sealed class ProviderProfileUpdatedDomainEventHandler(
    IMessageBus messageBus,
    ProvidersDbContext context,
    ILogger<ProviderProfileUpdatedDomainEventHandler> logger) : IEventHandler<ProviderProfileUpdatedDomainEvent>
{
    /// <summary>
    /// Processa o evento de domínio de perfil de prestador atualizado de forma assíncrona.
    /// </summary>
    public async Task HandleAsync(ProviderProfileUpdatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Handling ProviderProfileUpdatedDomainEvent for provider {ProviderId}", domainEvent.AggregateId);

            // Busca apenas o UserId do prestador
            var userId = await context.Providers
                .AsNoTracking()
                .Where(p => p.Id == new Domain.ValueObjects.ProviderId(domainEvent.AggregateId))
                .Select(p => (Guid?)p.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!userId.HasValue)
            {
                logger.LogWarning("Provider {ProviderId} not found when handling ProviderProfileUpdatedDomainEvent", domainEvent.AggregateId);
                return;
            }

            // Cria evento de integração para sistemas externos usando mapper
            var integrationEvent = domainEvent.ToIntegrationEvent(userId.Value, domainEvent.UpdatedFields);

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully published ProviderProfileUpdated integration event for provider {ProviderId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ProviderProfileUpdatedDomainEvent for provider {ProviderId}", domainEvent.AggregateId);
            throw new InvalidOperationException(
                $"Failed to publish ProviderProfileUpdated integration event for provider '{domainEvent.AggregateId}'",
                ex);
        }
    }
}
