using MeAjudaAi.Modules.SearchProviders.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.SearchProviders.Application.DTOs;

/// <summary>
/// DTO representando um provedor pesquisável nos resultados de busca.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record SearchableProviderDto(
    Guid ProviderId,
    string Name,
    LocationDto Location,
    decimal AverageRating,
    int TotalReviews,
    ESubscriptionTier SubscriptionTier,
    IReadOnlyList<Guid> ServiceIds,
    string Slug,
    string? Description = null,
    double? DistanceInKm = null,
    string? City = null,
    string? State = null);
