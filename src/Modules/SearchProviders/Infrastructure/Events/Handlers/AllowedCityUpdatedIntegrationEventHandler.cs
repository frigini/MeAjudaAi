using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Modules.SearchProviders.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Locations;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;

/// <summary>
/// Handler para processar o evento de atualização de cidade permitida.
/// Reindexa todos os provedores da cidade para garantir que dados estejam atualizados.
/// </summary>
internal sealed class AllowedCityUpdatedIntegrationEventHandler(
    ISearchProvidersModuleApi searchProvidersModuleApi,
    ISearchableProviderQueries queries,
    ILogger<AllowedCityUpdatedIntegrationEventHandler> logger) : IEventHandler<AllowedCityUpdatedIntegrationEvent>
{
    public async Task HandleAsync(AllowedCityUpdatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Handling AllowedCityUpdated for {CityName}/{StateSigla}. Reindexing providers in this city...",
            integrationEvent.CityName,
            integrationEvent.StateSigla);

        var providers = await queries.GetByCityIdAsync(integrationEvent.CityId, track: false, cancellationToken);

        if (providers.Count == 0)
        {
            logger.LogInformation("No providers found in city {CityId} ({CityName}), nothing to reindex", integrationEvent.CityId, integrationEvent.CityName);
            return;
        }

        var providerIds = providers.Select(provider => provider.ProviderId);
        foreach (var providerId in providerIds)
        {
            var indexResult = await searchProvidersModuleApi.IndexProviderAsync(providerId, cancellationToken);
            if (indexResult.IsFailure)
            {
                logger.LogError(
                    "Failed to reindex provider {ProviderId} after city update: {Error}",
                    providerId,
                    indexResult.Error.Message);
            }
        }

        logger.LogInformation(
            "Finished processing AllowedCityUpdated for {CityName}/{StateSigla}. Reindexed {Count} providers.",
            integrationEvent.CityName,
            integrationEvent.StateSigla,
            providers.Count);
    }
}
