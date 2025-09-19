using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

/// <summary>
/// Domain event triggered when a user's email address is changed.
/// </summary>
/// <remarks>
/// This event is published when a user's email is updated through the ChangeEmail method.
/// Can be used for synchronization with external systems (like Keycloak),
/// email verification workflows, notification services, etc.
/// Important: Email changes may require re-authentication in some systems.
/// </remarks>
/// <param name="AggregateId">Unique identifier of the user whose email was changed</param>
/// <param name="Version">Version of the aggregate when the event occurred</param>
/// <param name="OldEmail">Previous email address</param>
/// <param name="NewEmail">New email address</param>
public record UserEmailChangedEvent(
    Guid AggregateId,
    int Version,
    string OldEmail,
    string NewEmail
) : DomainEvent(AggregateId, Version);