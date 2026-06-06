namespace MeAjudaAi.Contracts.Modules.Payments.DTOs;

public record ModuleSubscriptionDto(
    Guid SubscriptionId,
    Guid ProviderId,
    string PlanId,
    string Status,
    DateTime? ExpiresAt
);
