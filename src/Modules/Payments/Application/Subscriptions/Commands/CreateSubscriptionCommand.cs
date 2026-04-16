using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;

public record CreateSubscriptionCommand(
    Guid ProviderId,
    string PlanId
) : ICommand<string>
{
    public Guid CorrelationId { get; init; } = UuidGenerator.NewId();
}
