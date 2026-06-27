using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Modules.SearchProviders.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Locations;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Infrastructure.Events.Handlers;

/// <summary>
/// Handler para processar o evento de criação de cidade permitida.
/// Reindexa todos os provedores da cidade para garantir que estejam no índice de busca.
/// </summary>
internal sealed class AllowedCityCreatedIntegrationEventHandler(
    ISearchProvidersModuleApi searchProvidersModuleApi,
    ISearchableProviderQueries queries,
    ILogger<AllowedCityCreatedIntegrationEventHandler> logger) : IEventHandler<AllowedCityCreatedIntegrationEvent>
{
    public async Task HandleAsync(AllowedCityCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Handling AllowedCityCreated for {CityName}/{StateSigla}. Reindexing providers in this city...",
            integrationEvent.CityName,
            integrationEvent.StateSigla);

        // Buscar todos os provedores ativos desta cidade (com UF para evitar cidades homônimas)
        var providers = await queries.GetByCityAndStateSiglaAsync(integrationEvent.CityName, integrationEvent.StateSigla, track: false, cancellationToken);

        if (providers.Count == 0)
        {
            logger.LogInformation("No providers found in {CityName}/{StateSigla}, nothing to reindex", integrationEvent.CityName, integrationEvent.StateSigla);
            return;
        }

        var providerIds = providers.Select(provider => provider.ProviderId);
        var successCount = 0;
        foreach (var providerId in providerIds)
        {
            var indexResult = await searchProvidersModuleApi.IndexProviderAsync(providerId, cancellationToken);
            if (indexResult.IsFailure)
            {
                logger.LogError(
                    "Failed to reindex provider {ProviderId} after city creation: {Error}",
                    providerId,
                    indexResult.Error.Message);
            }
            else
            {
                successCount++;
            }
        }

        logger.LogInformation(
            "Finished processing AllowedCityCreated for {CityName}/{StateSigla}. Reindexed {Count}/{Total} providers.",
            integrationEvent.CityName,
            integrationEvent.StateSigla,
            successCount,
            providers.Count);
    }
}
