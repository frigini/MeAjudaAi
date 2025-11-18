using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Commands.Service;

/// <summary>
/// Command to update an existing service's details.
/// </summary>
public sealed record UpdateServiceCommand(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder
) : Command<Result>;
