namespace MeAjudaAi.Modules.Catalogs.Application.DTOs;

/// <summary>
/// DTO for category with its services count.
/// </summary>
public sealed record ServiceCategoryWithCountDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    int DisplayOrder,
    int ActiveServicesCount,
    int TotalServicesCount
);
