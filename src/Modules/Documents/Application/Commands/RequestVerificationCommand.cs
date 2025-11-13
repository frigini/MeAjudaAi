using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Documents.Application.Commands;

public record RequestVerificationCommand(Guid DocumentId) : ICommand
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
