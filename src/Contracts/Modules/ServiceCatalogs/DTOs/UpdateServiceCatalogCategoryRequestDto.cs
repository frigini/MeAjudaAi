namespace MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs;

/// <summary>
/// Request DTO para atualização de categoria de serviço.
/// </summary>
public sealed record UpdateServiceCatalogCategoryRequestDto(
    string Name,
    string? Description,
    int DisplayOrder);
