using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

/// <summary>
/// Domain event triggered when a user's username is changed.
/// </summary>
/// <remarks>
/// This event is published when a user's username is updated through the ChangeUsername method.
/// Can be used for synchronization with external systems (like Keycloak),
/// username uniqueness validation, audit trails, notification services, etc.
/// Important: Username changes may affect authentication and should be handled carefully.
/// </remarks>
/// <param name="AggregateId">Unique identifier of the user whose username was changed</param>
/// <param name="Version">Version of the aggregate when the event occurred</param>
/// <param name="OldUsername">Previous username</param>
/// <param name="NewUsername">New username</param>
public record UserUsernameChangedEvent(
    Guid AggregateId,
    int Version,
    Username OldUsername,
    Username NewUsername
) : DomainEvent(AggregateId, Version);