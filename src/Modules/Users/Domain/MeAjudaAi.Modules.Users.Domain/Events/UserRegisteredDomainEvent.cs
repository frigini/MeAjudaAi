using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

public record UserRegisteredDomainEvent(
    Guid AggregateId,
    int Version,
    string Email,
    string FirstName,
    string LastName
) : DomainEvent(AggregateId, Version);