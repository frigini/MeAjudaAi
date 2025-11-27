using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Events;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Mappers;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Contracts.Modules;
using MeAjudaAi.Shared.Contracts.Modules.SearchProviders;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers;

/// <summary>
/// Manipula eventos de domínio ProviderVerificationStatusUpdatedDomainEvent e publica eventos de integração.
/// Integra com Search module para indexar providers verificados.
/// </summary>
public sealed class ProviderVerificationStatusUpdatedDomainEventHandler(
    IMessageBus messageBus,
    ProvidersDbContext context,
    ISearchProvidersModuleApi searchProvidersModuleApi,
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

            // Integração com SearchProviders Module: indexar provider quando verificado
            if (domainEvent.NewStatus == EVerificationStatus.Verified)
            {
                logger.LogInformation("Provider {ProviderId} verified, indexing in SearchProviders module", domainEvent.AggregateId);

                var indexResult = await searchProvidersModuleApi.IndexProviderAsync(provider.Id.Value, cancellationToken);
                if (indexResult.IsFailure)
                {
                    logger.LogError("Failed to index provider {ProviderId} in search: {Error}",
                        domainEvent.AggregateId, indexResult.Error);
                    // Não falhar o handler - busca pode ser reindexada depois via background job
                }
                else
                {
                    logger.LogInformation("Provider {ProviderId} indexed in SearchProviders successfully", domainEvent.AggregateId);
                }
            }
            else if (domainEvent.NewStatus == EVerificationStatus.Rejected || domainEvent.NewStatus == EVerificationStatus.Suspended)
            {
                logger.LogInformation("Provider {ProviderId} status changed to {Status}, removing from search index", 
                    domainEvent.AggregateId, domainEvent.NewStatus);

                var removeResult = await searchProvidersModuleApi.RemoveProviderAsync(provider.Id.Value, cancellationToken);
                if (removeResult.IsFailure)
                {
                    logger.LogError("Failed to remove provider {ProviderId} from search: {Error}",
                        domainEvent.AggregateId, removeResult.Error);
                    // Não falhar o handler - remoção pode ser feita depois via background job
                }
                else
                {
                    logger.LogInformation("Provider {ProviderId} removed from SearchProviders index successfully", domainEvent.AggregateId);
                }
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
