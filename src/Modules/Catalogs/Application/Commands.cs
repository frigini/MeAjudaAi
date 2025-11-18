using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Commands;

// ============================================================================
// SERVICE CATEGORY COMMANDS
// ============================================================================

public sealed record CreateServiceCategoryCommand(
    string Name,
    string? Description,
    int DisplayOrder = 0
) : Command<Result<ServiceCategoryDto>>;

public sealed record UpdateServiceCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder
) : Command<Result>;

public sealed record DeleteServiceCategoryCommand(Guid Id) : Command<Result>;

public sealed record ActivateServiceCategoryCommand(Guid Id) : Command<Result>;

public sealed record DeactivateServiceCategoryCommand(Guid Id) : Command<Result>;

// ============================================================================
// SERVICE COMMANDS
// ============================================================================

/// <summary>
/// Command to create a new service in a specific category.
/// </summary>
public sealed record CreateServiceCommand(
    Guid CategoryId,
    string Name,
    string? Description,
    int DisplayOrder = 0
) : Command<Result<ServiceDto>>;

/// <summary>
/// Command to update an existing service's details.
/// </summary>
public sealed record UpdateServiceCommand(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder
) : Command<Result>;

/// <summary>
/// Command to delete a service from the catalog.
/// Note: Currently does not check for provider references (see handler TODO).
/// </summary>
public sealed record DeleteServiceCommand(Guid Id) : Command<Result>;

/// <summary>
/// Command to activate a service, making it available for use.
/// </summary>
public sealed record ActivateServiceCommand(Guid Id) : Command<Result>;

/// <summary>
/// Command to deactivate a service, removing it from active use.
/// </summary>
public sealed record DeactivateServiceCommand(Guid Id) : Command<Result>;

/// <summary>
/// Command to move a service to a different category.
/// </summary>
public sealed record ChangeServiceCategoryCommand(
    Guid ServiceId,
    Guid NewCategoryId
) : Command<Result>;
