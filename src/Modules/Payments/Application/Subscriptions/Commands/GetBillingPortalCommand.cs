using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Utilities;

using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Payments.Application.Subscriptions.Commands;

[ExcludeFromCodeCoverage]
public record GetBillingPortalCommand(Guid ProviderId, string ReturnUrl) : ICommand<string>
{
    public Guid CorrelationId { get; init; } = UuidGenerator.NewId();
}

