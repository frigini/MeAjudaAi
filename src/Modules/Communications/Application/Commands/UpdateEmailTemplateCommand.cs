using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Communications.Application.Commands;

public sealed record UpdateEmailTemplateCommand(
    Guid Id,
    string Subject,
    string HtmlBody,
    string TextBody,
    Guid CorrelationId) : ICommand<Result>;
