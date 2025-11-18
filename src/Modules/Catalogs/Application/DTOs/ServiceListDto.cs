namespace MeAjudaAi.Modules.Catalogs.Application.DTOs;

/// <summary>
/// Simplified DTO for service without category details (for lists).
/// </summary>
public sealed record ServiceListDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    string? Description,
    bool IsActive
);
