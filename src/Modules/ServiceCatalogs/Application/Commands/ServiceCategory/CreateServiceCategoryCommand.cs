using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;

/// <summary>
/// Comando para criar uma nova categoria de serviço no catálogo.
/// </summary>
/// <param name="Name"></param>
/// <param name="Description"></param>
/// <param name="DisplayOrder"></param>
public sealed record CreateServiceCategoryCommand(
    string Name,
    string? Description,
    int DisplayOrder = 0
) : Command<Result<ServiceCategoryDto>>;