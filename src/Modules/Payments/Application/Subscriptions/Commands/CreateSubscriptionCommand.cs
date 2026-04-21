using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Utilities;

using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;

[ExcludeFromCodeCoverage]
public record CreateSubscriptionCommand(
    Guid ProviderId,
    string PlanId,
    string? IdempotencyKey = null
) : ICommand<string>
{
    public Guid CorrelationId { get; init; } = UuidGenerator.NewId();
}

