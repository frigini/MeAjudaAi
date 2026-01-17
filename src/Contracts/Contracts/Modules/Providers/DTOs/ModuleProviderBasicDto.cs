namespace MeAjudaAi.Contracts.Modules.Providers.DTOs;

/// <summary>
/// DTO com informações básicas do provider para comunicação entre módulos
/// </summary>
public sealed record ModuleProviderBasicDto(
    Guid Id,
    string Name,
    string Email,
    string ProviderType,
    string VerificationStatus,
    bool IsActive);

