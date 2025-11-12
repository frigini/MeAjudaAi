using MeAjudaAi.Modules.Providers.Domain.Enums;

namespace MeAjudaAi.Modules.Providers.Application.DTOs;

/// <summary>
/// DTO para prestador de serviços.
/// </summary>
public sealed record ProviderDto(
    Guid Id,
    Guid UserId,
    string Name,
    EProviderType Type,
    BusinessProfileDto BusinessProfile,
    EProviderStatus Status,
    EVerificationStatus VerificationStatus,
    IReadOnlyList<DocumentDto> Documents,
    IReadOnlyList<QualificationDto> Qualifications,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsDeleted,
    DateTime? DeletedAt
);
