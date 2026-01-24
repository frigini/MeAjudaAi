using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Contracts.Utilities.Constants;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Mappings;

/// <summary>
/// Métodos de extensão para mapear entidades de domínio para DTOs.
/// Centraliza a lógica de mapeamento para evitar duplicação entre handlers.
/// </summary>
public static class ServiceCatalogsMappingExtensions
{
    /// <summary>
    /// Mapeia uma entidade Service para ServiceListDto.
    /// </summary>
    public static ServiceListDto ToListDto(this Service service)
        => new(
            service.Id.Value,
            service.CategoryId.Value,
            service.Name,
            service.Description,
            service.IsActive);

    /// <summary>
    /// Mapeia uma entidade Service para ServiceDto.
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
    /// Mapeia uma entidade ServiceCategory para ServiceCategoryDto.
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
