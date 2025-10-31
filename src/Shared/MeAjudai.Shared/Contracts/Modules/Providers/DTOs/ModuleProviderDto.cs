namespace MeAjudaAi.Shared.Contracts.Modules.Providers.DTOs;

/// <summary>
/// DTO básico de prestador para comunicação entre módulos
/// </summary>
public sealed record ModuleProviderDto(
    Guid Id,
    Guid UserId,
    string Name,
    EProviderType Type,
    EVerificationStatus VerificationStatus,
    ModuleProviderContactDto ContactInfo,
    ModuleProviderLocationDto Location,
    DateTime CreatedAt,
    bool IsActive
);

/// <summary>
/// DTO básico de informações de contato do prestador para comunicação entre módulos
/// </summary>
public sealed record ModuleProviderContactDto(
    string Email,
    string? PhoneNumber,
    string? Website
);

/// <summary>
/// DTO básico de localização do prestador para comunicação entre módulos
/// </summary>
public sealed record ModuleProviderLocationDto(
    string City,
    string State,
    string Country
);

/// <summary>
/// DTO simplificado de prestador para operações em lote
/// </summary>
public sealed record ModuleProviderBasicDto(
    Guid Id,
    Guid UserId,
    string Name,
    EProviderType Type,
    bool IsActive
);