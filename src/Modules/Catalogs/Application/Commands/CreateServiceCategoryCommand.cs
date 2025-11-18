using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Commands;

public sealed record CreateServiceCategoryCommand(
    string Name,
    string? Description,
    int DisplayOrder = 0
) : Command<Result<Guid>>;
