using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Shared.Messaging.Messages.Users;

/// <summary>
/// Published when a new user registers in the system
/// </summary>
public record UserRegistered(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string KeycloakId,
    List<string> Roles,
    DateTime RegisteredAt
) : IntegrationEvent("Users");

/// <summary>
/// Published when a user updates their profile information
/// </summary>
public record UserProfileUpdated(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    DateTime UpdatedAt
) : IntegrationEvent("Users");

/// <summary>
/// Published when a user account is deactivated
/// </summary>
public record UserDeactivated(
    Guid UserId,
    string Email,
    string Reason,
    DateTime DeactivatedAt
) : IntegrationEvent("Users");

/// <summary>
/// Published when a user's role changes
/// </summary>
public record UserRoleChanged(
    Guid UserId,
    string Email,
    string PreviousRole,
    string NewRole,
    string ChangedBy,
    DateTime ChangedAt
) : IntegrationEvent("Users");

/// <summary>
/// Published when a user account is locked out due to security reasons
/// </summary>
public record UserLockedOut(
    Guid UserId,
    string Email,
    string Reason,
    DateTime LockedOutAt,
    DateTime? UnlockAt
) : IntegrationEvent("Users");

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