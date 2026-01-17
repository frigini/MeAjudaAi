using MeAjudaAi.Contracts.Modules.SearchProviders.Enums;

namespace MeAjudaAi.Contracts.Modules.SearchProviders.DTOs;

/// <summary>
/// DTO de prestador pesquisável para a API do módulo.
/// </summary>
public sealed record ModuleSearchableProviderDto(
    Guid ProviderId,
    string Name,
    ModuleLocationDto Location,
    decimal AverageRating,
    int TotalReviews,
    ESubscriptionTier SubscriptionTier,
    IReadOnlyCollection<Guid> ServiceIds,
    string? Description = null,
    double? DistanceInKm = null,
    string? City = null,
    string? State = null);

