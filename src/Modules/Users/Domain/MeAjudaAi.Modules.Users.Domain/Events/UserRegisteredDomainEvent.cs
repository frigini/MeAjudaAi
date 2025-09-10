using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

/// <summary>
/// Published when a new user registers
/// </summary>
public record UserRegisteredDomainEvent(
    Guid AggregateId,
    int Version,
    string Email,
    Username Username,
    string FirstName,
    string LastName
) : DomainEvent(AggregateId, Version);