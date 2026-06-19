using MeAjudaAi.Modules.Communications.Application.Commands;
using MeAjudaAi.Modules.Communications.Application.DTOs;

namespace MeAjudaAi.Modules.Communications.API.Mappers;

/// <summary>
/// Métodos de extensão para mapear DTOs para Commands do módulo Communications.
/// </summary>
public static class RequestMapperExtensions
{
    /// <summary>
    /// Mapeia UpdateEmailTemplateBody para UpdateEmailTemplateCommand.
    /// </summary>
    public static UpdateEmailTemplateCommand ToCommand(this UpdateEmailTemplateBody body, Guid id, Guid correlationId)
    {
        return new UpdateEmailTemplateCommand(
            id,
            body.Subject,
            body.HtmlBody,
            body.TextBody,
            correlationId);
    }

    /// <summary>
    /// Mapeia um Guid para SetEmailTemplateStatusCommand (ativação).
    /// </summary>
    public static SetEmailTemplateStatusCommand ToActivateCommand(this Guid id, Guid correlationId)
    {
        return new SetEmailTemplateStatusCommand(id, true, correlationId);
    }

    /// <summary>
    /// Mapeia um Guid para SetEmailTemplateStatusCommand (desativação).
    /// </summary>
    public static SetEmailTemplateStatusCommand ToDeactivateCommand(this Guid id, Guid correlationId)
    {
        return new SetEmailTemplateStatusCommand(id, false, correlationId);
    }
}
