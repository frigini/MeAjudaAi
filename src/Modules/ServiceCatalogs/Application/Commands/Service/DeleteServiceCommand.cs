using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;

/// <summary>
/// Command to delete a service from the catalog.
/// Note: Future enhancement required - implement soft-delete pattern (IsActive = false) to preserve
/// audit history and prevent deletion when providers reference this service. See handler TODO.
/// </summary>
public sealed record DeleteServiceCommand(Guid Id) : Command<Result>;
