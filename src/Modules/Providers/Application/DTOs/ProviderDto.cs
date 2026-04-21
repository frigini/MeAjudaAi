using MeAjudaAi.Modules.Providers.Domain.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.Application.DTOs;

/// <summary>
/// DTO para prestador de serviços.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ProviderDto(
    Guid Id,
    Guid UserId,
    string Name,
    string Slug,
    EProviderType Type,
    BusinessProfileDto BusinessProfile,
    EProviderStatus Status,
    EVerificationStatus VerificationStatus,
    EProviderTier Tier,
    IReadOnlyList<DocumentDto> Documents,
    IReadOnlyList<QualificationDto> Qualifications,
    IReadOnlyList<ProviderServiceDto> Services,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsDeleted,
    DateTime? DeletedAt,
    bool IsActive,
    string? SuspensionReason = null,
    string? RejectionReason = null
);
