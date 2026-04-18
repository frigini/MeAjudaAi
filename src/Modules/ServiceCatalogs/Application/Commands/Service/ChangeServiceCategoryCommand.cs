using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;

/// <summary>
/// Comando para mover um serviço para uma categoria diferente.
/// </summary>
public sealed record ChangeServiceCategoryCommand(
    Guid ServiceId,
    Guid NewCategoryId
) : Command<Result>;

