namespace MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;

/// <summary>
/// DTO para categoria com a contagem de seus serviços.
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
