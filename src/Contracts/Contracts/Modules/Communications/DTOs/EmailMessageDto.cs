namespace MeAjudaAi.Contracts.Modules.Communications.DTOs;

/// <summary>
/// DTO para envio de mensagem de e-mail.
/// </summary>
public sealed record EmailMessageDto(
    string To,
    string Subject,
    string Body,
    bool IsHtml = true,
    string? TemplateKey = null,
    IDictionary<string, string>? TemplateData = null,
    IEnumerable<string>? AttachmentUrls = null
);
