using MeAjudaAi.Modules.SearchProviders.Application.DTOs;
using MeAjudaAi.Modules.SearchProviders.Application.Queries;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.Repositories;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Contracts.Modules;
using MeAjudaAi.Shared.Contracts.Modules.Providers;
using MeAjudaAi.Shared.Contracts.Modules.SearchProviders;
using MeAjudaAi.Shared.Contracts.Modules.SearchProviders.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;
using DomainEnums = MeAjudaAi.Modules.SearchProviders.Domain.Enums;

namespace MeAjudaAi.Modules.SearchProviders.Application.ModuleApi;

/// <summary>
/// Implementação da API pública do módulo SearchProviders para outros módulos.
/// </summary>
[ModuleApi(ModuleMetadata.Name, ModuleMetadata.Version)]
public sealed class SearchProvidersModuleApi(
    IQueryDispatcher queryDispatcher,
    ISearchableProviderRepository repository,
    IProvidersModuleApi providersApi,
    ILogger<SearchProvidersModuleApi> logger) : ISearchProvidersModuleApi
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
        ESubscriptionTier[]? subscriptionTiers = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Mapear enums do módulo para enums do domínio usando mapeamento explícito
        DomainEnums.ESubscriptionTier[]? domainTiers = subscriptionTiers?.Select(ToDomainTier).ToArray();

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

    public async Task<Result> IndexProviderAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Indexing provider {ProviderId} in search", providerId);

            // 1. Buscar dados completos do provider via IProvidersModuleApi
            var providerDataResult = await providersApi.GetProviderForIndexingAsync(providerId, cancellationToken);

            if (providerDataResult.IsFailure)
            {
                logger.LogError("Failed to get provider data for indexing provider {ProviderId}: {Error}",
                    providerId, providerDataResult.Error.Message);
                return Result.Failure(providerDataResult.Error);
            }

            var providerData = providerDataResult.Value;
            if (providerData == null)
            {
                logger.LogWarning("Provider {ProviderId} not found in Providers module, cannot index", providerId);
                return Result.Failure($"Provider {providerId} not found");
            }

            // 2. Verificar se provider já existe no índice
            var existing = await repository.GetByProviderIdAsync(providerId, cancellationToken);

            if (existing != null)
            {
                logger.LogDebug("Provider {ProviderId} already indexed, updating", providerId);

                // Atualizar informações do provider existente
                var location = new GeoPoint(providerData.Latitude, providerData.Longitude);
                existing.UpdateBasicInfo(providerData.Name, providerData.Description, providerData.City, providerData.State);
                existing.UpdateLocation(location);
                existing.UpdateRating(providerData.AverageRating, providerData.TotalReviews);
                existing.UpdateSubscriptionTier(ToDomainTier(providerData.SubscriptionTier));
                existing.UpdateServices(providerData.ServiceIds);

                if (providerData.IsActive)
                    existing.Activate();
                else
                    existing.Deactivate();

                await repository.UpdateAsync(existing, cancellationToken);
            }
            else
            {
                logger.LogDebug("Creating new search index entry for provider {ProviderId}", providerId);

                // Criar novo SearchableProvider
                var location = new GeoPoint(providerData.Latitude, providerData.Longitude);
                var searchableProvider = SearchableProvider.Create(
                    providerId: providerData.ProviderId,
                    name: providerData.Name,
                    location: location,
                    subscriptionTier: ToDomainTier(providerData.SubscriptionTier),
                    description: providerData.Description,
                    city: providerData.City,
                    state: providerData.State);

                // Atualizar dados adicionais
                searchableProvider.UpdateRating(providerData.AverageRating, providerData.TotalReviews);
                searchableProvider.UpdateServices(providerData.ServiceIds);

                if (!providerData.IsActive)
                    searchableProvider.Deactivate();

                await repository.AddAsync(searchableProvider, cancellationToken);
            }

            await repository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Provider {ProviderId} indexed successfully in search", providerId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error indexing provider {ProviderId} in search", providerId);
            return Result.Failure("Failed to index provider due to an internal error");
        }
    }

    public async Task<Result> RemoveProviderAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Removing provider {ProviderId} from search index", providerId);

            var existing = await repository.GetByProviderIdAsync(providerId, cancellationToken);

            if (existing == null)
            {
                logger.LogDebug("Provider {ProviderId} not found in search index, nothing to remove", providerId);
                return Result.Success(); // Idempotent: já removido ou nunca indexado
            }

            await repository.DeleteAsync(existing, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Provider {ProviderId} removed from search index successfully", providerId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing provider {ProviderId} from search index", providerId);
            return Result.Failure("Failed to remove provider due to an internal error");
        }
    }

    /// <summary>
    /// Mapeia o enum de tier do módulo para o enum de tier do domínio com conversão explícita.
    /// </summary>
    private static DomainEnums.ESubscriptionTier ToDomainTier(ESubscriptionTier tier) => tier switch
    {
        ESubscriptionTier.Free => DomainEnums.ESubscriptionTier.Free,
        ESubscriptionTier.Standard => DomainEnums.ESubscriptionTier.Standard,
        ESubscriptionTier.Gold => DomainEnums.ESubscriptionTier.Gold,
        ESubscriptionTier.Platinum => DomainEnums.ESubscriptionTier.Platinum,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown subscription tier")
    };

    /// <summary>
    /// Mapeia o enum de tier do domínio para o enum de tier do módulo com conversão explícita.
    /// </summary>
    private static ESubscriptionTier ToModuleTier(DomainEnums.ESubscriptionTier tier) => tier switch
    {
        DomainEnums.ESubscriptionTier.Free => ESubscriptionTier.Free,
        DomainEnums.ESubscriptionTier.Standard => ESubscriptionTier.Standard,
        DomainEnums.ESubscriptionTier.Gold => ESubscriptionTier.Gold,
        DomainEnums.ESubscriptionTier.Platinum => ESubscriptionTier.Platinum,
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown subscription tier")
    };
}
