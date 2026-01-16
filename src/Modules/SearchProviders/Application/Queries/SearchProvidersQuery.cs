using System.Globalization;
using MeAjudaAi.Modules.SearchProviders.Application.DTOs;
using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using MeAjudaAi.Contracts;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.SearchProviders.Application.Queries;

/// <summary>
/// Query to search for providers based on location, services, and other criteria.
/// </summary>
public sealed record SearchProvidersQuery(
    double Latitude,
    double Longitude,
    double RadiusInKm,
    Guid[]? ServiceIds = null,
    decimal? MinRating = null,
    ESubscriptionTier[]? SubscriptionTiers = null,
    int Page = 1,
    int PageSize = 20
) : Query<Result<PagedResult<SearchableProviderDto>>>, ICacheableQuery
{
    public string GetCacheKey()
    {
        // Arredonda coordenadas para 4 casas decimais (~11m de precisão) para melhor cache hit rate
        var lat = Latitude.ToString("F4", CultureInfo.InvariantCulture);
        var lng = Longitude.ToString("F4", CultureInfo.InvariantCulture);
        var radius = RadiusInKm.ToString("G", CultureInfo.InvariantCulture);
        var rating = (MinRating ?? 0).ToString("G", CultureInfo.InvariantCulture);

        // Ordena e concatena service IDs para cache consistency
        var serviceKey = ServiceIds != null && ServiceIds.Length > 0
            ? string.Join("-", ServiceIds.OrderBy(x => x))
            : "all";

        // Ordena subscription tiers para cache consistency
        var tierKey = SubscriptionTiers != null && SubscriptionTiers.Length > 0
            ? string.Join("-", SubscriptionTiers.OrderBy(x => x))
            : "all";

        return $"search:providers:lat:{lat}:lng:{lng}:radius:{radius}:services:{serviceKey}:rating:{rating}:tiers:{tierKey}:page:{Page}:size:{PageSize}";
    }

    public TimeSpan GetCacheExpiration()
    {
        // Cache curto (5 minutos) devido a atualizações frequentes de:
        // - Localização de prestadores
        // - Ratings de reviews
        // - Status de assinatura
        return TimeSpan.FromMinutes(5);
    }

    public IReadOnlyCollection<string>? GetCacheTags()
    {
        return ["search", "providers", "search-results"];
    }
}
