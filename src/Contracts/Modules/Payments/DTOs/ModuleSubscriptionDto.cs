using System.Text.Json.Serialization;
using MeAjudaAi.Contracts.Modules.Payments.Enums;

namespace MeAjudaAi.Contracts.Modules.Payments.DTOs;

public record ModuleSubscriptionDto(
    Guid SubscriptionId,
    Guid ProviderId,
    string PlanId,
    ESubscriptionStatus Status,
    DateTime? ExpiresAt
)
{
    [Obsolete("Use Status instead.")]
    [JsonPropertyName("SubscriptionStatus")]
    public ESubscriptionStatus SubscriptionStatus => Status;
};
