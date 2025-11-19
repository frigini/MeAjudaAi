using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Commands.Service;

/// <summary>
/// Command to deactivate a service, removing it from active use.
/// </summary>
public sealed record DeactivateServiceCommand(Guid Id) : Command<Result>;
