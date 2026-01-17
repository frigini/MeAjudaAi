namespace MeAjudaAi.Contracts.Modules.Providers.DTOs;

/// <summary>
/// DTO completo do provider para comunicação entre módulos
/// </summary>
public sealed record ModuleProviderDto(
    Guid Id,
    string Name,
    string Email,
    string Document,
    string ProviderType,
    string VerificationStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsActive,
    string? Phone = null);

