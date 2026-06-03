namespace MeAjudaAi.Contracts.Modules.Communications.DTOs;

/// <summary>
/// DTO de template de e-mail.
/// </summary>
public sealed record EmailTemplateDto(
    Guid Id,
    string Key,
    string Subject,
    string HtmlBody,
    string TextBody,
    bool IsSystemTemplate,
    string Language
);
