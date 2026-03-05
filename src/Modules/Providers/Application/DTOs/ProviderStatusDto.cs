using MeAjudaAi.Modules.Providers.Domain.Enums;

namespace MeAjudaAi.Modules.Providers.Application.DTOs;

/// <summary>
/// DTO leve para consulta de status de aprovação e tier do prestador.
/// Usado pelo endpoint GET /api/v1/providers/me/status.
/// </summary>
public sealed record ProviderStatusDto(
    EProviderStatus Status,
    EProviderTier Tier,
    EVerificationStatus VerificationStatus,
    string? RejectionReason
);
