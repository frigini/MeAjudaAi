using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Documents.Application.Commands;

/// <summary>
/// Comando para excluir um documento e seu blob associado.
/// </summary>
/// <param name="DocumentId">ID do documento a ser excluído</param>
public record DeleteDocumentCommand(Guid DocumentId) : ICommand<Result>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
