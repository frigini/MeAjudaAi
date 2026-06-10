using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Communications.Application.Commands;

public sealed record SetEmailTemplateStatusCommand(
    Guid Id,
    bool IsActive,
    Guid CorrelationId) : ICommand<Result>;
