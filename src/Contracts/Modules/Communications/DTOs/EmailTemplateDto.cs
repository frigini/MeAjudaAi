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
    bool IsActive,
    bool IsSystemTemplate,
    string Language,
    int Version,
    string? OverrideKey = null
);
