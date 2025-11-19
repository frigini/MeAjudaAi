using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;

public sealed record UpdateServiceCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder = 0
) : Command<Result>;
