using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;

/// <summary>
/// Comando para criar um novo serviço em uma categoria específica.
/// </summary>
public sealed record CreateServiceCommand(
    Guid CategoryId,
    string Name,
    string? Description,
    int DisplayOrder = 0
) : Command<Result<ServiceDto>>;