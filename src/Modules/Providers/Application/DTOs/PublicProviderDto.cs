using MeAjudaAi.Modules.Providers.Domain.Enums;

namespace MeAjudaAi.Modules.Providers.Application.DTOs;

/// <summary>
/// DTO seguro para exibição pública de dados do prestador.
/// Remove informações sensíveis como documentos, motivo de rejeição, etc.
/// </summary>
public sealed record PublicProviderDto(
    Guid Id,
    string Name,
    EProviderType Type,
    string? FantasyName,
    string? Description,
    string? City,
    string? State,
    DateTime CreatedAt,
    
    // Dados para UI refinada
    double? Rating,
    int ReviewCount,
    IEnumerable<string> Services,
    IEnumerable<string> PhoneNumbers,
    string? Email
);
