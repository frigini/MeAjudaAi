using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Utilities;

using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Payments.Application.Commands;

[ExcludeFromCodeCoverage]
public record CreateBillingPortalSessionCommand(Guid ProviderId, string ReturnUrl) : ICommand<string>
{
    public Guid CorrelationId { get; init; } = UuidGenerator.NewId();
}
