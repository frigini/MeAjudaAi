using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Utilities;

namespace MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;

public record GetBillingPortalCommand(Guid ProviderId, string ReturnUrl) : ICommand<string>
{
    public Guid CorrelationId { get; init; } = UuidGenerator.NewId();
}
