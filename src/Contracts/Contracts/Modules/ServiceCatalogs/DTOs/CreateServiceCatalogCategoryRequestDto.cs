namespace MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs;

/// <summary>
/// Request DTO para criação de categoria de serviço.
/// </summary>
public sealed record CreateServiceCatalogCategoryRequestDto(
    string Name,
    string? Description,
    int DisplayOrder);
