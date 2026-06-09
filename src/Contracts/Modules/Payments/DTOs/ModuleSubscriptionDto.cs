using MeAjudaAi.Contracts.Modules.Payments.Enums;

namespace MeAjudaAi.Contracts.Modules.Payments.DTOs;

public record ModuleSubscriptionDto(
    Guid SubscriptionId,
    Guid ProviderId,
    string PlanId,
    SubscriptionStatus Status,
    DateTime? ExpiresAt
);
