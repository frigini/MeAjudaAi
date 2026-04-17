using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Documents.Application.Commands;

/// <summary>
/// Comando para rejeitar um documento após verificação manual.
/// </summary>
/// <param name="DocumentId">ID do documento a ser rejeitado</param>
/// <param name="RejectionReason">Motivo da rejeição</param>
/// <param name="VerificationNotes">Notas adicionais da verificação (opcional)</param>
[ExcludeFromCodeCoverage]
public record RejectDocumentCommand(
    Guid DocumentId, 
    string RejectionReason, 
    string? VerificationNotes = null) : ICommand<Result>
{
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
