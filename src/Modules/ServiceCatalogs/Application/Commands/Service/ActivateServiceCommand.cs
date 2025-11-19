using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;

/// <summary>
/// Command to activate a service, making it available for use.
/// </summary>
public sealed record ActivateServiceCommand(Guid Id) : Command<Result>;
