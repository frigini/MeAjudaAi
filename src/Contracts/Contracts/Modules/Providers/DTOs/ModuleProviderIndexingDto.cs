using MeAjudaAi.Contracts.Modules.SearchProviders.Enums;

namespace MeAjudaAi.Contracts.Modules.Providers.DTOs;

/// <summary>
/// DTO otimizado para indexação de providers no módulo SearchProviders.
/// Contém todos os dados necessários para criar/atualizar um SearchableProvider.
/// </summary>
public sealed record ModuleProviderIndexingDto(
    Guid ProviderId,
    string Name,
    double Latitude,
    double Longitude,
    IReadOnlyCollection<Guid> ServiceIds,
    decimal AverageRating,
    int TotalReviews,
    ESubscriptionTier SubscriptionTier,
    bool IsActive,
    string? Description = null,
    string? City = null,
    string? State = null);

