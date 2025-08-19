using MeAjudaAi.Shared.Events;

namespace MeAjudaAi.Modules.Users.Domain.Events;

public sealed record UserSubscriptionUpdatedDomainEvent(
    Guid UserId,
    int Version,
    string SubscriptionId,
    string Status,
    DateTime? ExpiresAt,
    DateTime UpdatedAt = default
) : DomainEvent(UserId, Version)
{
    public DateTime UpdatedAt { get; init; } = UpdatedAt == default ? DateTime.UtcNow : UpdatedAt;
}