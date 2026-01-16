using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.Documents.Application.Commands;

public record RequestVerificationCommand(Guid DocumentId) : ICommand<Result>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
