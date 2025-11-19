using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;

/// <summary>
/// Command to create a new service in a specific category.
/// </summary>
public sealed record CreateServiceCommand(
    Guid CategoryId,
    string Name,
    string? Description,
    int DisplayOrder = 0
) : Command<Result<ServiceDto>>;
