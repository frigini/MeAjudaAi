using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;

/// <summary>
/// Command to activate a service category.
/// </summary>
public sealed record ActivateServiceCategoryCommand(Guid Id) : Command<Result>;
