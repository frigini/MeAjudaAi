using MeAjudaAi.Modules.SearchProviders.Domain.Enums;

namespace MeAjudaAi.Modules.SearchProviders.Application.DTOs;

/// <summary>
/// DTO representando um provedor pesquisável nos resultados de busca.
/// </summary>
public sealed record SearchableProviderDto(
    Guid ProviderId,
    string Name,
    LocationDto Location,
    decimal AverageRating,
    int TotalReviews,
    ESubscriptionTier SubscriptionTier,
    IReadOnlyList<Guid> ServiceIds,
    string? Description = null,
    double? DistanceInKm = null,
    string? City = null,
    string? State = null);
