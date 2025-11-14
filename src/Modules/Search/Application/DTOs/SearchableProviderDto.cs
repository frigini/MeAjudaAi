using MeAjudaAi.Modules.Search.Domain.Enums;

namespace MeAjudaAi.Modules.Search.Application.DTOs;

/// <summary>
/// DTO representing a searchable provider in search results.
/// </summary>
public sealed record SearchableProviderDto
{
    /// <summary>
    /// Provider's unique identifier.
    /// </summary>
    public required Guid ProviderId { get; init; }

    /// <summary>
    /// Provider's name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Provider's description/bio.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Geographic coordinates.
    /// </summary>
    public required LocationDto Location { get; init; }

    /// <summary>
    /// Average customer rating (0-5).
    /// </summary>
    public decimal AverageRating { get; init; }

    /// <summary>
    /// Total number of reviews.
    /// </summary>
    public int TotalReviews { get; init; }

    /// <summary>
    /// Subscription tier.
    /// </summary>
    public ESubscriptionTier SubscriptionTier { get; init; }

    /// <summary>
    /// List of service IDs offered by this provider.
    /// </summary>
    public Guid[] ServiceIds { get; init; } = Array.Empty<Guid>();

    /// <summary>
    /// Distance from search location in kilometers.
    /// Only populated when searching by location.
    /// </summary>
    public double? DistanceInKm { get; init; }

    /// <summary>
    /// City where the provider is located.
    /// </summary>
    public string? City { get; init; }

    /// <summary>
    /// State/province where the provider is located.
    /// </summary>
    public string? State { get; init; }
}

/// <summary>
/// DTO representing geographic coordinates.
/// </summary>
public sealed record LocationDto
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
}
