namespace MeAjudaAi.Shared.Contracts.Modules.Providers.DTOs;

/// <summary>
/// Request DTO para atualização do status de verificação de um provider.
/// </summary>
public sealed record UpdateVerificationStatusRequestDto(
    string VerificationStatus,
    string? Reason = null);
