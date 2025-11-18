using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Commands.ServiceCategory;

public sealed record ActivateServiceCategoryCommand(Guid Id) : Command<Result>;
