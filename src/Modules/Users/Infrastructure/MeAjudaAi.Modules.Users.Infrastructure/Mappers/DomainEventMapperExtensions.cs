using MeAjudaAi.Modules.Users.Domain.Events;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Messaging.Messages.Users;

namespace MeAjudaAi.Modules.Users.Infrastructure.Mappers;

/// <summary>
/// Extension methods for mapping Domain Events to Integration Events
/// </summary>
public static class DomainEventMapperExtensions
{
    /// <summary>
    /// Maps UserRegisteredDomainEvent to UserRegisteredIntegrationEvent
    /// </summary>
    /// <param name="domainEvent">The domain event to map</param>
    /// <returns>Integration event for cross-module communication</returns>
    public static UserRegisteredIntegrationEvent ToIntegrationEvent(this UserRegisteredDomainEvent domainEvent)
    {
        return new UserRegisteredIntegrationEvent(
            Source: "Users",
            UserId: domainEvent.AggregateId,
            Email: domainEvent.Email,
            Username: domainEvent.Username.Value,
            FirstName: domainEvent.FirstName,
            LastName: domainEvent.LastName,
            KeycloakId: string.Empty, // Will be filled by infrastructure layer
            Roles: Array.Empty<string>(), // Will be filled by infrastructure layer
            RegisteredAt: DateTime.UtcNow
        );
    }

    /// <summary>
    /// Maps UserProfileUpdatedDomainEvent to UserProfileUpdatedIntegrationEvent
    /// </summary>
    /// <param name="domainEvent">The domain event to map</param>
    /// <param name="email">The user's email (must be provided from the user repository)</param>
    /// <returns>Integration event for cross-module communication</returns>
    public static UserProfileUpdatedIntegrationEvent ToIntegrationEvent(this UserProfileUpdatedDomainEvent domainEvent, string email)
    {
        return new UserProfileUpdatedIntegrationEvent(
            Source: "Users",
            UserId: domainEvent.AggregateId,
            Email: email,
            FirstName: domainEvent.FirstName,
            LastName: domainEvent.LastName,
            UpdatedAt: DateTime.UtcNow
        );
    }

    /// <summary>
    /// Maps UserDeletedDomainEvent to UserDeletedIntegrationEvent
    /// </summary>
    /// <param name="domainEvent">The domain event to map</param>
    /// <returns>Integration event for cross-module communication</returns>
    public static UserDeletedIntegrationEvent ToIntegrationEvent(this UserDeletedDomainEvent domainEvent)
    {
        return new UserDeletedIntegrationEvent(
            Source: "Users",
            UserId: domainEvent.AggregateId,
            DeletedAt: DateTime.UtcNow
        );
    }
}