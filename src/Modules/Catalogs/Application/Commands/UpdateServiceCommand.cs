using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Commands;

public sealed record UpdateServiceCommand(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder
) : Command<Result>;
