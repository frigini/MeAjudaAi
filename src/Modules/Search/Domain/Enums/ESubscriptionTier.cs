namespace MeAjudaAi.Modules.Search.Domain.Enums;

/// <summary>
/// Represents the subscription tier of a provider.
/// Higher tiers receive better positioning in search results.
/// </summary>
public enum ESubscriptionTier
{
    /// <summary>
    /// Free tier - basic listing
    /// </summary>
    Free = 0,

    /// <summary>
    /// Standard tier - enhanced listing with additional features
    /// </summary>
    Standard = 1,

    /// <summary>
    /// Gold tier - premium listing with priority in search results
    /// </summary>
    Gold = 2,

    /// <summary>
    /// Platinum tier - highest priority in search results with maximum visibility
    /// </summary>
    Platinum = 3
}
