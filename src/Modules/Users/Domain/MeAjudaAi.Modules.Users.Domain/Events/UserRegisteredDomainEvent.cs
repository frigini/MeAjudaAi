using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

/// <summary>
/// Published when a new user registers
/// </summary>
public record UserRegisteredDomainEvent(
    Guid AggregateId,
    int Version,
    string Email,
    string FirstName,
    string LastName
) : DomainEvent(AggregateId, Version);