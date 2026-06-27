using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.SearchProviders.DTOs;
using MeAjudaAi.Contracts.Modules.SearchProviders.Enums;
using Refit;

namespace MeAjudaAi.Client.Contracts.Api;

public interface ISearchProvidersApi
{
    [Get($"{ApiEndpoints.VersionPrefix}/{ApiEndpoints.SearchProviders.Providers}")]
    Task<PagedResult<ModuleSearchableProviderDto>> SearchProvidersAsync(
        [Query] double latitude,
        [Query] double longitude,
        [Query] double radiusInKm,
        [Query] string? term = null,
        [Query] Guid[]? serviceIds = null,
        [Query] decimal? minRating = null,
        [Query] ESubscriptionTier[]? subscriptionTiers = null,
        [Query] int page = 1,
        [Query] int pageSize = 20,
        CancellationToken cancellationToken = default);
}
