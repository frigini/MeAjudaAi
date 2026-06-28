using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;

/// <summary>
/// Comando para ativar uma categoria de serviço.
/// </summary>
public sealed record ActivateServiceCategoryCommand(Guid Id) : Command<Result>;