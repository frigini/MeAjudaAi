using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;

public sealed record DeleteServiceCategoryCommand(Guid Id) : Command<Result>;
