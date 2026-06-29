using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Contracts.Modules.Documents.DTOs;

/// <summary>
/// Request para verificação de documento (aprovar ou rejeitar).
/// </summary>
/// <param name="IsVerified">True para aprovar, False para rejeitar.</param>
/// <param name="VerificationNotes">Notas da verificação (obrigatório ao rejeitar).</param>
[ExcludeFromCodeCoverage]
public sealed record VerifyDocumentRequest(bool IsVerified, string? VerificationNotes);
