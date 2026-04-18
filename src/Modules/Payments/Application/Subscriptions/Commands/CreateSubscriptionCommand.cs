using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;

public record CreateSubscriptionCommand(
    Guid ProviderId,
    string PlanId,
    string? IdempotencyKey = null
) : ICommand<string>
{
    public Guid CorrelationId { get; init; } = UuidGenerator.NewId();
}

