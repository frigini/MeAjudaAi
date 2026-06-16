using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Documents.Application.Commands;

public record RequestVerificationCommand(Guid DocumentId) : ICommand<Result>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
