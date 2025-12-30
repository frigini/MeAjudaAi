using MeAjudaAi.Shared.Contracts.Modules.SearchProviders.Enums;

namespace MeAjudaAi.Shared.Contracts.Modules.SearchProviders.DTOs;

/// <summary>
/// Searchable provider DTO for module API.
/// </summary>
public sealed record ModuleSearchableProviderDto
{
    public required Guid ProviderId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required ModuleLocationDto Location { get; init; }
    public decimal AverageRating { get; init; }
    public int TotalReviews { get; init; }
    public ESubscriptionTier SubscriptionTier { get; init; }
    public IReadOnlyCollection<Guid> ServiceIds { get; init; } = Array.Empty<Guid>();
    public double? DistanceInKm { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
}

