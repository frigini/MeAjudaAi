using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Documents.Application.Commands;

/// <summary>
/// Comando para aprovar um documento após verificação manual.
/// </summary>
/// <param name="DocumentId">ID do documento a ser aprovado</param>
/// <param name="VerificationNotes">Notas da verificação (opcional)</param>
public record ApproveDocumentCommand(Guid DocumentId, string? VerificationNotes = null) : ICommand<Result>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
