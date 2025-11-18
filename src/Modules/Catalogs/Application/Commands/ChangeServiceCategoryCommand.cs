using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Commands;

public sealed record ChangeServiceCategoryCommand(
    Guid ServiceId,
    Guid NewCategoryId
) : Command<Result>;
