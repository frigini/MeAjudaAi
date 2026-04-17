using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;

/// <summary>
/// Comando para mover um serviço para uma categoria diferente.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ChangeServiceCategoryCommand(
    Guid ServiceId,
    Guid NewCategoryId
) : Command<Result>;
