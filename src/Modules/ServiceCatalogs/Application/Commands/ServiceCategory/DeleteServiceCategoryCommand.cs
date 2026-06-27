using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;

/// <summary>
/// Comando para deletar uma categoria de serviço do catálogo.
/// </summary>
/// <param name="Id"></param>
public sealed record DeleteServiceCategoryCommand(Guid Id) : Command<Result>;