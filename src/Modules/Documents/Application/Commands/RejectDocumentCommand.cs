using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Documents.Application.Commands;

/// <summary>
/// Comando para rejeitar um documento após verificação manual.
/// </summary>
/// <param name="DocumentId">ID do documento a ser rejeitado</param>
/// <param name="RejectionReason">Motivo da rejeição</param>
/// <param name="VerificationNotes">Notas adicionais da verificação (opcional)</param>
public record RejectDocumentCommand(
    Guid DocumentId, 
    string RejectionReason, 
    string? VerificationNotes = null) : ICommand<Result>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
