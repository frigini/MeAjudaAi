using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules;
using MeAjudaAi.Contracts.Modules.Locations;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Modules.SearchProviders;
using MeAjudaAi.Contracts.Modules.SearchProviders.DTOs;
using MeAjudaAi.Contracts.Modules.SearchProviders.Enums;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.SearchProviders.Application.DTOs;
using MeAjudaAi.Modules.SearchProviders.Application.Mappers;
using MeAjudaAi.Modules.SearchProviders.Application.Queries;
using MeAjudaAi.Modules.SearchProviders.Application.Queries.Interfaces;
using MeAjudaAi.Modules.SearchProviders.Domain.Entities;
using MeAjudaAi.Modules.SearchProviders.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Constants;
using MeAjudaAi.Shared.Geolocation;
using MeAjudaAi.Shared.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DomainEnums = MeAjudaAi.Modules.SearchProviders.Domain.Enums;

namespace MeAjudaAi.Modules.SearchProviders.Application.ModuleApi;

/// <summary>
/// Implementação da API pública do módulo SearchProviders para outros módulos.
/// </summary>
[ModuleApi(ModuleMetadata.Name, ModuleMetadata.Version)]
public sealed class SearchProvidersModuleApi(
    IQueryDispatcher queryDispatcher,
    [FromKeyedServices(ModuleKeys.SearchProviders)] IUnitOfWork uow,
    ISearchableProviderQueries queries,
    IProvidersModuleApi providersApi,
    ILocationsModuleApi locationsApi,
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
        return await queries.CanConnectAsync(cancellationToken);
    }

    public async Task<Result<ModulePagedSearchResultDto>> SearchProvidersAsync(
        double latitude,
        double longitude,
        double radiusInKm,
        string? term = null,
        Guid[]? serviceIds = null,
        decimal? minRating = null,
        ESubscriptionTier[]? subscriptionTiers = null,
        int pageNumber = Pagination.DefaultPageNumber,
        int pageSize = Pagination.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        // Mapear enums do módulo para enums do domínio usando mapeamento explícito
        DomainEnums.ESubscriptionTier[]? domainTiers = subscriptionTiers?.Select(t => t.ToDomainTier()).ToArray();

        var query = new SearchProvidersQuery(
            latitude,
            longitude,
            radiusInKm,
            term,
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
        var moduleResult = new ModulePagedSearchResultDto(
            Items: result.Value!.Items.Select(p => new ModuleSearchableProviderDto(
                ProviderId: p.ProviderId,
                Name: p.Name,
                Slug: p.Slug,
                Location: new ModuleLocationDto(
                    Latitude: p.Location.Latitude,
                    Longitude: p.Location.Longitude),
                AverageRating: p.AverageRating,
                TotalReviews: p.TotalReviews,
                SubscriptionTier: p.SubscriptionTier.ToModuleTier(),
                ServiceIds: p.ServiceIds,
                Description: p.Description,
                DistanceInKm: p.DistanceInKm,
                City: p.City,
                State: p.State))
            .ToList(),
            TotalCount: result.Value.TotalItems,
            PageNumber: result.Value.PageNumber,
            PageSize: result.Value.PageSize);

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

            // Lookup CityId
            Guid? cityId = null;
            if (!string.IsNullOrWhiteSpace(providerData.City) && !string.IsNullOrWhiteSpace(providerData.State))
            {
                try
                {
                    var cityIdResult = await locationsApi.GetAllowedCityIdAsync(providerData.City, providerData.State, cancellationToken);
                    if (cityIdResult.IsSuccess)
                    {
                        cityId = cityIdResult.Value;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to lookup CityId for {City}/{State} during indexing of provider {ProviderId}, continuing without CityId",
                        providerData.City, providerData.State, providerId);
                }
            }

            // 2. Verificar se provider já existe no índice
            var existing = await queries.GetByProviderIdAsync(providerId, track: true, cancellationToken);

            if (existing != null)
            {
                logger.LogDebug("Provider {ProviderId} already indexed, updating", providerId);

                // Atualizar informações do provider existente
                var location = new GeoPoint(providerData.Latitude, providerData.Longitude);
                existing.UpdateBasicInfo(providerData.Name, providerData.Slug, providerData.Description, providerData.City, providerData.State, cityId);
                existing.UpdateLocation(location);
                existing.UpdateRating(providerData.AverageRating, providerData.TotalReviews);
                existing.UpdateSubscriptionTier(providerData.SubscriptionTier.ToDomainTier());
                existing.UpdateServices(providerData.ServiceIds.ToArray());

                if (providerData.IsActive)
                    existing.Activate();
                else
                    existing.Deactivate();
            }
            else
            {
                logger.LogDebug("Creating new search index entry for provider {ProviderId}", providerId);

                // Criar novo SearchableProvider
                var location = new GeoPoint(providerData.Latitude, providerData.Longitude);
                var searchableProvider = SearchableProvider.Create(
                    providerId: providerData.ProviderId,
                    name: providerData.Name,
                    slug: providerData.Slug,
                    location: location,
                    subscriptionTier: providerData.SubscriptionTier.ToDomainTier(),
                    description: providerData.Description,
                    city: providerData.City,
                    state: providerData.State,
                    cityId: cityId);

                // Atualizar dados adicionais
                searchableProvider.UpdateRating(providerData.AverageRating, providerData.TotalReviews);
                searchableProvider.UpdateServices(providerData.ServiceIds.ToArray());

                if (!providerData.IsActive)
                    searchableProvider.Deactivate();

                uow.GetRepository<SearchableProvider, SearchableProviderId>().Add(searchableProvider);
            }

            await uow.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Provider {ProviderId} indexed successfully in search", providerId);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException)
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

            var existing = await queries.GetByProviderIdAsync(providerId, track: true, cancellationToken);

            if (existing == null)
            {
                logger.LogDebug("Provider {ProviderId} not found in search index, nothing to remove", providerId);
                return Result.Success(); // Idempotent: já removido ou nunca indexado
            }

            uow.GetRepository<SearchableProvider, SearchableProviderId>().Delete(existing);
            await uow.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Provider {ProviderId} removed from search index successfully", providerId);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException)
        {
            logger.LogError(ex, "Error removing provider {ProviderId} from search index", providerId);
            return Result.Failure("Failed to remove provider due to an internal error");
        }
    }
}
