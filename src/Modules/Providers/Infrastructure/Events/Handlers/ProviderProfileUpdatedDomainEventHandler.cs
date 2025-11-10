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

            // Busca o prestador para garantir que temos os dados mais recentes
            var provider = await context.Providers
                .FirstOrDefaultAsync(p => p.Id == new Domain.ValueObjects.ProviderId(domainEvent.AggregateId), cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found when handling ProviderProfileUpdatedDomainEvent", domainEvent.AggregateId);
                return;
            }

            // Cria evento de integração para sistemas externos usando mapper
            var updatedFields = new[] { "Name", "Email" }; // Determinar quais campos foram atualizados
            var integrationEvent = domainEvent.ToIntegrationEvent(provider.UserId, updatedFields);

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully published ProviderProfileUpdated integration event for provider {ProviderId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ProviderProfileUpdatedDomainEvent for provider {ProviderId}", domainEvent.AggregateId);
            throw;
        }
    }
}