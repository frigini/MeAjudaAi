using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;

/// <summary>
/// Comando para desativar uma categoria de serviço.
/// </summary>
/// <param name="Id"></param>
public sealed record DeactivateServiceCategoryCommand(Guid Id) : Command<Result>;
