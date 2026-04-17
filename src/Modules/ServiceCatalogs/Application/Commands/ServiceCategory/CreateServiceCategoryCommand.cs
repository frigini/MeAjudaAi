using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;

[ExcludeFromCodeCoverage]

public sealed record CreateServiceCategoryCommand(
    string Name,
    string? Description,
    int DisplayOrder = 0
) : Command<Result<ServiceCategoryDto>>;
