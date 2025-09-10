using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

/// <summary>
/// Domain event emitted when a user is deleted (soft delete)
/// </summary>
public record UserDeletedDomainEvent(
    Guid AggregateId,
    int Version
) : DomainEvent(AggregateId, Version);