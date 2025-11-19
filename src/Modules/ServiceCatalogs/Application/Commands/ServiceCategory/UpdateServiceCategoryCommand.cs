using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;

/// <summary>
/// Command to update an existing service category's information.
/// Note: This command requires all fields for updates (full-update pattern).
/// Future enhancement: Consider supporting partial updates where clients only send changed fields
/// using nullable fields or optional wrapper types if API requirements evolve.
/// </summary>
public sealed record UpdateServiceCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder = 0
) : Command<Result>;
