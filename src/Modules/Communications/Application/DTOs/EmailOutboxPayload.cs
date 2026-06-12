namespace MeAjudaAi.Modules.Communications.Application.DTOs;

/// <summary>
/// Payload para mensagens de e-mail no outbox.
/// </summary>
public sealed record EmailOutboxPayload(
    string To, 
    string Subject, 
    string? HtmlBody = null, 
    string? TextBody = null, 
    string? Body = null, 
    string? From = null, 
    string? TemplateKey = null,
    IDictionary<string, string>? TemplateData = null);