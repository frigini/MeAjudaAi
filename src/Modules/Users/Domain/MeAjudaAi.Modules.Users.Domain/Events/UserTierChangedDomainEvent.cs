using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

public sealed record UserTierChangedDomainEvent(
    Guid UserId,
    int Version,
    string PreviousTier,
    string NewTier,
    string ChangedBy,
    DateTime ChangedAt = default
) : DomainEvent(UserId, Version)
{
    public DateTime ChangedAt { get; init; } = ChangedAt == default ? DateTime.UtcNow : ChangedAt;
}