using MeAjudaAi.Modules.Search.Domain.Enums;
using MeAjudaAi.Modules.Search.Domain.ValueObjects;
using MeAjudaAi.Shared.Domain;
using MeAjudaAi.Shared.Geolocation;

namespace MeAjudaAi.Modules.Search.Domain.Entities;

/// <summary>
/// Read model for provider search.
/// Denormalized entity optimized for geolocation queries and ranking.
/// </summary>
public sealed class SearchableProvider : AggregateRoot<SearchableProviderId>
{
    /// <summary>
    /// Reference to the original provider ID in the Providers module.
    /// </summary>
    public Guid ProviderId { get; private set; }

    /// <summary>
    /// Provider's name for display in search results.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Provider's geographic location.
    /// </summary>
    public GeoPoint Location { get; private set; } = null!;

    /// <summary>
    /// Average rating from customer reviews (0-5).
    /// </summary>
    public decimal AverageRating { get; private set; }

    /// <summary>
    /// Total number of reviews received.
    /// </summary>
    public int TotalReviews { get; private set; }

    /// <summary>
    /// Current subscription tier affecting search ranking.
    /// </summary>
    public ESubscriptionTier SubscriptionTier { get; private set; }

    /// <summary>
    /// List of service IDs this provider offers.
    /// Stored as array for efficient filtering.
    /// </summary>
    public Guid[] ServiceIds { get; private set; } = Array.Empty<Guid>();

    /// <summary>
    /// Indicates if the provider is currently active and should appear in search results.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Provider's description/bio for search result display.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// City where the provider is located.
    /// </summary>
    public string? City { get; private set; }

    /// <summary>
    /// State/province where the provider is located.
    /// </summary>
    public string? State { get; private set; }

    // Private constructor for EF Core
    private SearchableProvider()
    {
    }

    private SearchableProvider(
        SearchableProviderId id,
        Guid providerId,
        string name,
        GeoPoint location,
        ESubscriptionTier subscriptionTier) : base(id)
    {
        ProviderId = providerId;
        Name = name;
        Location = location ?? throw new ArgumentNullException(nameof(location));
        SubscriptionTier = subscriptionTier;
        AverageRating = 0;
        TotalReviews = 0;
        IsActive = true;
        ServiceIds = Array.Empty<Guid>();
    }

    /// <summary>
    /// Creates a new searchable provider entry.
    /// </summary>
    public static SearchableProvider Create(
        Guid providerId,
        string name,
        GeoPoint location,
        ESubscriptionTier subscriptionTier = ESubscriptionTier.Free,
        string? description = null,
        string? city = null,
        string? state = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Provider name cannot be empty.", nameof(name));
        }

        if (location is null)
        {
            throw new ArgumentNullException(nameof(location));
        }

        var searchableProvider = new SearchableProvider(
            SearchableProviderId.New(),
            providerId,
            name.Trim(),
            location,
            subscriptionTier)
        {
            Description = description?.Trim(),
            City = city?.Trim(),
            State = state?.Trim()
        };

        return searchableProvider;
    }

    /// <summary>
    /// Updates provider's basic information.
    /// </summary>
    public void UpdateBasicInfo(string name, string? description, string? city, string? state)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Provider name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
        Description = description?.Trim();
        City = city?.Trim();
        State = state?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates provider's location.
    /// </summary>
    public void UpdateLocation(GeoPoint location)
    {
        Location = location ?? throw new ArgumentNullException(nameof(location));
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates provider's rating based on new review data.
    /// </summary>
    public void UpdateRating(decimal averageRating, int totalReviews)
    {
        if (averageRating < 0 || averageRating > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(averageRating), "Rating must be between 0 and 5.");
        }

        if (totalReviews < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalReviews), "Total reviews cannot be negative.");
        }

        AverageRating = averageRating;
        TotalReviews = totalReviews;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates provider's subscription tier.
    /// </summary>
    public void UpdateSubscriptionTier(ESubscriptionTier tier)
    {
        SubscriptionTier = tier;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the list of services offered by the provider.
    /// </summary>
    public void UpdateServices(Guid[] serviceIds)
    {
        ServiceIds = serviceIds?.ToArray() ?? Array.Empty<Guid>();
        MarkAsUpdated();
    }

    /// <summary>
    /// Activates the provider in search results.
    /// </summary>
    public void Activate()
    {
        if (IsActive) return;
        
        IsActive = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Deactivates the provider from search results.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive) return;
        
        IsActive = false;
        MarkAsUpdated();
    }

    /// <summary>
    /// Calculates distance to a given location in kilometers.
    /// </summary>
    public double CalculateDistanceToInKm(GeoPoint targetLocation)
    {
        if (targetLocation is null)
            throw new ArgumentNullException(nameof(targetLocation));

        return Location.DistanceTo(targetLocation);
    }
}
