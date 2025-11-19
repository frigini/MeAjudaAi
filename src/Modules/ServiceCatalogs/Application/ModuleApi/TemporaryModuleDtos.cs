// TODO Phase 2: Remove this file when proper shared contracts are added in MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs.DTOs
// These are temporary placeholders to allow Phase 1b to compile without breaking the module API pattern

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.ModuleApi;

/// <summary>
/// Temporary DTO - will be replaced by shared contract in Phase 2
/// </summary>
public sealed record ModuleServiceCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    int DisplayOrder
);

/// <summary>
/// Temporary DTO - will be replaced by shared contract in Phase 2
/// </summary>
public sealed record ModuleServiceDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string? Description,
    bool IsActive,
    int DisplayOrder
);

/// <summary>
/// Temporary DTO - will be replaced by shared contract in Phase 2
/// </summary>
public sealed record ModuleServiceListDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive
);

/// <summary>
/// Temporary DTO - will be replaced by shared contract in Phase 2
/// </summary>
public sealed record ModuleServiceValidationResultDto(
    bool AllValid,
    IReadOnlyCollection<Guid> InvalidServiceIds,
    IReadOnlyCollection<Guid> InactiveServiceIds
);
