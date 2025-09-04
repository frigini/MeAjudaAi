using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.ServiceProvider;

/// <summary>
/// Published when a user becomes a service provider
/// </summary>
public record ServiceProviderCreated(
    Guid UserId,
    Guid ServiceProviderId,
    string CompanyName,
    string Tier,
    DateTime CreatedAt
) : IntegrationEvent("Users");

/// <summary>
/// Published when a service provider's tier changes
/// </summary>
public record ServiceProviderTierChanged(
    Guid UserId,
    Guid ServiceProviderId,
    string CompanyName,
    string PreviousTier,
    string NewTier,
    string ChangedBy,
    DateTime ChangedAt
) : IntegrationEvent("Users");

/// <summary>
/// Published when a service provider gets verified
/// </summary>
public record ServiceProviderVerified(
    Guid UserId,
    Guid ServiceProviderId,
    string CompanyName,
    string VerifiedBy,
    DateTime VerifiedAt
) : IntegrationEvent("Users");

/// <summary>
/// Published when a service provider's subscription status changes
/// </summary>
public record ServiceProviderSubscriptionUpdated(
    Guid UserId,
    Guid ServiceProviderId,
    string SubscriptionId,
    string Status,
    DateTime? ExpiresAt,
    DateTime UpdatedAt
) : IntegrationEvent("Users");