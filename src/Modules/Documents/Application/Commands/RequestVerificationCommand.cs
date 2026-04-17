using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Documents.Application.Commands;

[ExcludeFromCodeCoverage]

public record RequestVerificationCommand(Guid DocumentId) : ICommand<Result>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
