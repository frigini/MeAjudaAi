using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Commands;

namespace MeAjudaAi.Modules.Communications.Application.Commands;

public sealed record CreateEmailTemplateCommand(
    string Key,
    string Subject,
    string HtmlBody,
    string TextBody,
    bool IsSystemTemplate,
    string Language,
    Guid CorrelationId) : ICommand<Result<Guid>>;
