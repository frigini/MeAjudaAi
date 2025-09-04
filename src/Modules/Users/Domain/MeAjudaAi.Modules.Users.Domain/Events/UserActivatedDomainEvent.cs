using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

/// <summary>
/// Published when a user account is activated
/// </summary>
public sealed record UserActivatedDomainEvent(
    Guid AggregateId,
    int Version,
    string ActivatedBy // who activated (admin, system, self)
) : DomainEvent(AggregateId, Version);