using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;

/// <summary>
/// Command to delete a service from the catalog.
/// Note: Currently does not check for provider references (see handler TODO).
/// </summary>
public sealed record DeleteServiceCommand(Guid Id) : Command<Result>;
