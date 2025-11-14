using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Mappers;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;

/// <summary>
/// Manipula eventos de domínio ProviderVerificationStatusUpdatedDomainEvent e publica eventos de integração.
/// </summary>
public sealed class ProviderVerificationStatusUpdatedDomainEventHandler(
    IMessageBus messageBus,
    ProvidersDbContext context,
    ILogger<ProviderVerificationStatusUpdatedDomainEventHandler> logger) : IEventHandler<ProviderVerificationStatusUpdatedDomainEvent>
{
    /// <summary>
    /// Processa o evento de domínio de status de verificação atualizado de forma assíncrona.
    /// </summary>
    public async Task HandleAsync(ProviderVerificationStatusUpdatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Handling ProviderVerificationStatusUpdatedDomainEvent for provider {ProviderId}", domainEvent.AggregateId);

            // Busca o prestador para garantir que temos os dados mais recentes
            var provider = await context.Providers
                .FirstOrDefaultAsync(p => p.Id == new Domain.ValueObjects.ProviderId(domainEvent.AggregateId), cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found when handling ProviderVerificationStatusUpdatedDomainEvent", domainEvent.AggregateId);
                return;
            }

            // Cria evento de integração para sistemas externos usando mapper
            var integrationEvent = domainEvent.ToIntegrationEvent(provider.UserId, provider.Name);

            await messageBus.PublishAsync(integrationEvent, cancellationToken: cancellationToken);

            logger.LogInformation("Successfully published ProviderVerificationStatusUpdated integration event for provider {ProviderId}", domainEvent.AggregateId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling ProviderVerificationStatusUpdatedDomainEvent for provider {ProviderId}", domainEvent.AggregateId);
            throw;
        }
    }
}
