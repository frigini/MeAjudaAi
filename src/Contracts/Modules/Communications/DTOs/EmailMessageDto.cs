namespace MeAjudaAi.Contracts.Modules.Communications.DTOs;

public sealed record EmailMessageDto(
    string To,
    string Subject,
    string? Body = null,
    bool IsHtml = true,
    string? TemplateKey = null,
    IDictionary<string, string>? TemplateData = null
);
