namespace MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs;

/// <summary>
/// DTO para informações de categoria de serviço exposto para outros módulos.
/// </summary>
public sealed record ModuleServiceCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    int DisplayOrder
);

