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

public sealed record CreateServiceCommand(
    Guid CategoryId,
    string Name,
    string? Description,
    int DisplayOrder = 0
) : Command<Result<ServiceDto>>;

public sealed record UpdateServiceCommand(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder
) : Command<Result>;

public sealed record DeleteServiceCommand(Guid Id) : Command<Result>;

public sealed record ActivateServiceCommand(Guid Id) : Command<Result>;

public sealed record DeactivateServiceCommand(Guid Id) : Command<Result>;

public sealed record ChangeServiceCategoryCommand(
    Guid ServiceId,
    Guid NewCategoryId
) : Command<Result>;
