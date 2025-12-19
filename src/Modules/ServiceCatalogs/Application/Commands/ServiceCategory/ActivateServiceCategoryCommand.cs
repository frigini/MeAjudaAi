using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;

/// <summary>
/// Comando para ativar uma categoria de serviço.
/// </summary>
public sealed record ActivateServiceCategoryCommand(Guid Id) : Command<Result>;
