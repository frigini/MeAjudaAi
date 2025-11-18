namespace MeAjudaAi.Modules.Catalogs.Application.DTOs;

/// <summary>
/// DTO for service category information.
/// </summary>
public sealed record ServiceCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    int DisplayOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
