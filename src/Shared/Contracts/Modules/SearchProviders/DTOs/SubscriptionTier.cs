namespace MeAjudaAi.Shared.Contracts.Modules.SearchProviders.DTOs;

/// <summary>
/// Subscription tier enumeration for module API.
/// Values must match MeAjudaAi.Modules.SearchProviders.Domain.Enums.ESubscriptionTier.
/// </summary>
public enum SubscriptionTier
{
    Free = 0,
    Standard = 1,
    Gold = 2,
    Platinum = 3
}
