using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;

namespace MeAjudaAi.Modules.Catalogs.Application.Commands;

public sealed record ActivateServiceCommand(Guid Id) : Command<Result>;
