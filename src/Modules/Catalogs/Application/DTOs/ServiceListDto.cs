namespace MeAjudaAi.Modules.Catalogs.Application.DTOs;

/// <summary>
/// DTO simplificado para serviço sem detalhes de categoria (para listas).
/// </summary>
public sealed record ServiceListDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    string? Description,
    bool IsActive
);
