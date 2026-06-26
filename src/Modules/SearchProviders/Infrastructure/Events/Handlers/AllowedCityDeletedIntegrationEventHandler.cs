using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Modules.SearchProviders.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Locations;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;

/// <summary>
/// Handler para processar o evento de remoção de cidade permitida.
/// Reindexa todos os provedores da cidade para atualizar seus dados.
/// </summary>
internal sealed class AllowedCityDeletedIntegrationEventHandler(
    ISearchProvidersModuleApi searchProvidersModuleApi,
    ISearchableProviderQueries queries,
    ILogger<AllowedCityDeletedIntegrationEventHandler> logger) : IEventHandler<AllowedCityDeletedIntegrationEvent>
{
    public async Task HandleAsync(AllowedCityDeletedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Handling AllowedCityDeleted for {CityName}/{StateSigla}. Reindexing providers in this city...",
            integrationEvent.CityName,
            integrationEvent.StateSigla);

        // Buscar todos os provedores ativos desta cidade
        var providers = await queries.GetByCityNameAsync(integrationEvent.CityName, track: false, cancellationToken);

        if (providers.Count == 0)
        {
            logger.LogInformation("No providers found in {CityName}, nothing to reindex", integrationEvent.CityName);
            return;
        }

        var providerIds = providers.Select(provider => provider.ProviderId);
        foreach (var providerId in providerIds)
        {
            var indexResult = await searchProvidersModuleApi.IndexProviderAsync(providerId, cancellationToken);
            if (indexResult.IsFailure)
            {
                logger.LogError(
                    "Failed to reindex provider {ProviderId} after city deletion: {Error}",
                    providerId,
                    indexResult.Error.Message);
            }
        }

        logger.LogInformation(
            "Finished processing AllowedCityDeleted for {CityName}. Reindexed {Count} providers.",
            integrationEvent.CityName,
            providers.Count);
    }
}
