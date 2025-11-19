namespace MeAjudaAi.Shared.Contracts.Modules.Search.DTOs;

/// <summary>
/// Subscription tier enumeration for module API.
/// Values must match MeAjudaAi.Modules.Search.Domain.Enums.ESubscriptionTier.
/// </summary>
public enum SubscriptionTier
{
    Free = 0,
    Standard = 1,
    Gold = 2,
    Platinum = 3
}
