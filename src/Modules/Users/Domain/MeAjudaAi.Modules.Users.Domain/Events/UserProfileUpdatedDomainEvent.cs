using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

public record UserProfileUpdatedDomainEvent(
    Guid AggregateId,
    int Version,
    string FirstName,
    string LastName
) : DomainEvent(AggregateId, Version);