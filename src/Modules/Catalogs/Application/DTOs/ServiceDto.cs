namespace MeAjudaAi.Modules.Catalogs.Application.DTOs;

/// <summary>
/// DTO for service information.
/// </summary>
public sealed record ServiceDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string? Description,
    bool IsActive,
    int DisplayOrder,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
