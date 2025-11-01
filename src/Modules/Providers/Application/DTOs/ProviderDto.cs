using MeAjudaAi.Modules.Providers.Domain.Enums;

namespace MeAjudaAi.Modules.Providers.Application.DTOs;

/// <summary>
/// DTO para informações de contato.
/// </summary>
public sealed record ContactInfoDto(
    string Email,
    string? PhoneNumber,
    string? Website
);

/// <summary>
/// DTO para endereço.
/// </summary>
public sealed record AddressDto(
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    string Country
);

/// <summary>
/// DTO para perfil empresarial.
/// </summary>
public sealed record BusinessProfileDto(
    string LegalName,
    string? FantasyName,
    string? Description,
    ContactInfoDto ContactInfo,
    AddressDto PrimaryAddress
);

/// <summary>
/// DTO para documento.
/// </summary>
public sealed record DocumentDto(
    string Number,
    EDocumentType DocumentType
);

/// <summary>
/// DTO para qualificação.
/// </summary>
public sealed record QualificationDto(
    string Name,
    string? Description,
    string? IssuingOrganization,
    DateTime? IssueDate,
    DateTime? ExpirationDate,
    string? DocumentNumber
);

/// <summary>
/// DTO para prestador de serviços.
/// </summary>
public sealed record ProviderDto(
    Guid Id,
    Guid UserId,
    string Name,
    EProviderType Type,
    BusinessProfileDto BusinessProfile,
    EVerificationStatus VerificationStatus,
    IReadOnlyList<DocumentDto> Documents,
    IReadOnlyList<QualificationDto> Qualifications,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsDeleted,
    DateTime? DeletedAt
);
