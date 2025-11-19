namespace MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs.DTOs;

/// <summary>
/// DTO for service category information exposed to other modules.
/// </summary>
public sealed record ModuleServiceCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    int DisplayOrder
);

/// <summary>
/// DTO for service information exposed to other modules.
/// </summary>
public sealed record ModuleServiceDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string? Description,
    bool IsActive
);

/// <summary>
/// Simplified service DTO for list operations.
/// </summary>
public sealed record ModuleServiceListDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    bool IsActive
);

/// <summary>
/// Result of service validation operation.
/// </summary>
public sealed record ModuleServiceValidationResultDto(
    bool AllValid,
    Guid[] InvalidServiceIds,
    Guid[] InactiveServiceIds
);
