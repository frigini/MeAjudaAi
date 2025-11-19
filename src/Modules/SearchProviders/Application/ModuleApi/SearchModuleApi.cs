using MeAjudaAi.Modules.SearchProviders.Application.DTOs;
using MeAjudaAi.Modules.SearchProviders.Application.Queries;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Contracts.Modules;
using MeAjudaAi.Shared.Contracts.Modules.Search;
using MeAjudaAi.Shared.Contracts.Modules.Search.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.SearchProviders.Application.ModuleApi;

/// <summary>
/// Implementação da API pública do módulo SearchProviders para outros módulos.
/// </summary>
[ModuleApi(ModuleMetadata.Name, ModuleMetadata.Version)]
public sealed class SearchModuleApi(
    IQueryDispatcher queryDispatcher,
    ILogger<SearchModuleApi> logger) : ISearchModuleApi
{
    private static class ModuleMetadata
    {
        public const string Name = "SearchProviders";
        public const string Version = "1.0";
    }

    public string ModuleName => ModuleMetadata.Name;
    public string ApiVersion => ModuleMetadata.Version;

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Checking SearchProviders module availability");

            // Teste básico: fazer uma busca com coordenadas válidas e radius pequeno
            var testResult = await SearchProvidersAsync(
                latitude: -23.561414,
                longitude: -46.656559,
                radiusInKm: 1.0,
                pageNumber: 1,
                pageSize: 1,
                cancellationToken: cancellationToken);

            // Módulo está disponível se conseguiu executar a busca (mesmo que retorne 0 resultados)
            if (testResult.IsSuccess)
            {
                logger.LogDebug("SearchProviders module is available and healthy");
            }
            else
            {
                logger.LogWarning("SearchProviders module test query failed");
            }
            return testResult.IsSuccess;
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("SearchProviders module availability check was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking SearchProviders module availability");
            return false;
        }
    }

    public async Task<Result<ModulePagedSearchResultDto>> SearchProvidersAsync(
        double latitude,
        double longitude,
        double radiusInKm,
        Guid[]? serviceIds = null,
        decimal? minRating = null,
        SubscriptionTier[]? subscriptionTiers = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Mapear enums do módulo para enums do domínio usando mapeamento explícito
        ESubscriptionTier[]? domainTiers = subscriptionTiers?.Select(ToDomainTier).ToArray();

        var query = new SearchProvidersQuery(
            latitude,
            longitude,
            radiusInKm,
            serviceIds,
            minRating,
            domainTiers,
            pageNumber,
            pageSize);

        var result = await queryDispatcher.QueryAsync<SearchProvidersQuery, Result<PagedResult<SearchableProviderDto>>>(query, cancellationToken);

        if (result.IsFailure)
        {
            return Result<ModulePagedSearchResultDto>.Failure(result.Error);
        }

        // Mapear DTOs internos para DTOs do módulo
        var moduleResult = new ModulePagedSearchResultDto
        {
            Items = result.Value!.Items.Select(p => new ModuleSearchableProviderDto
            {
                ProviderId = p.ProviderId,
                Name = p.Name,
                Description = p.Description,
                Location = new ModuleLocationDto
                {
                    Latitude = p.Location.Latitude,
                    Longitude = p.Location.Longitude
                },
                AverageRating = p.AverageRating,
                TotalReviews = p.TotalReviews,
                SubscriptionTier = ToModuleTier(p.SubscriptionTier),
                ServiceIds = p.ServiceIds,
                DistanceInKm = p.DistanceInKm,
                City = p.City,
                State = p.State
            }).ToList(),
            TotalCount = result.Value.TotalCount,
            PageNumber = result.Value.Page,
            PageSize = result.Value.PageSize
        };

        return Result<ModulePagedSearchResultDto>.Success(moduleResult);
    }

    /// <summary>
    /// Mapeia o enum de tier do módulo para o enum de tier do domínio com conversão explícita.
    /// </summary>
    private static ESubscriptionTier ToDomainTier(SubscriptionTier tier) => tier switch
    {
        SubscriptionTier.Free => ESubscriptionTier.Free,
        SubscriptionTier.Standard => ESubscriptionTier.Standard,
        SubscriptionTier.Gold => ESubscriptionTier.Gold,
        SubscriptionTier.Platinum => ESubscriptionTier.Platinum,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown subscription tier")
    };

    /// <summary>
    /// Mapeia o enum de tier do domínio para o enum de tier do módulo com conversão explícita.
    /// </summary>
    private static SubscriptionTier ToModuleTier(ESubscriptionTier tier) => tier switch
    {
        ESubscriptionTier.Free => SubscriptionTier.Free,
        ESubscriptionTier.Standard => SubscriptionTier.Standard,
        ESubscriptionTier.Gold => SubscriptionTier.Gold,
        ESubscriptionTier.Platinum => SubscriptionTier.Platinum,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown subscription tier")
    };
}
