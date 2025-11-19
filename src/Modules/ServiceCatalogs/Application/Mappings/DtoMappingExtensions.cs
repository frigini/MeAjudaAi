using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Shared.Constants;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Mappings;

/// <summary>
/// Extension methods for mapping domain entities to DTOs.
/// Centralizes mapping logic to avoid duplication across handlers.
/// </summary>
public static class DtoMappingExtensions
{
    /// <summary>
    /// Maps a Service entity to a ServiceListDto.
    /// </summary>
    public static ServiceListDto ToListDto(this Service service)
        => new(
            service.Id.Value,
            service.CategoryId.Value,
            service.Name,
            service.Description,
            service.IsActive);

    /// <summary>
    /// Maps a Service entity to a ServiceDto.
    /// </summary>
    public static ServiceDto ToDto(this Service service)
    {
        var categoryName = service.Category?.Name ?? ValidationMessages.Catalogs.UnknownCategoryName;
        
        return new ServiceDto(
            service.Id.Value,
            service.CategoryId.Value,
            categoryName,
            service.Name,
            service.Description,
            service.IsActive,
            service.DisplayOrder,
            service.CreatedAt,
            service.UpdatedAt);
    }

    /// <summary>
    /// Maps a ServiceCategory entity to a ServiceCategoryDto.
    /// </summary>
    public static ServiceCategoryDto ToDto(this ServiceCategory category)
        => new(
            category.Id.Value,
            category.Name,
            category.Description,
            category.IsActive,
            category.DisplayOrder,
            category.CreatedAt,
            category.UpdatedAt);
}
