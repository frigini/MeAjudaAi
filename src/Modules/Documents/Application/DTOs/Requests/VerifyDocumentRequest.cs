namespace MeAjudaAi.Modules.Documents.Application.DTOs.Requests;

/// <summary>
/// Request para verificação de documento.
/// </summary>
/// <param name="IsVerified">True para aprovar, False para rejeitar</param>
/// <param name="VerificationNotes">Notas da verificação</param>
public record VerifyDocumentRequest(bool IsVerified, string? VerificationNotes);
