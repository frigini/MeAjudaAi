namespace MeAjudaAi.Contracts.Modules.Communications.DTOs;

/// <summary>
/// Payload para mensagens de e-mail no outbox
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

/// <summary>
/// Payload para mensagens de SMS no outbox
/// </summary>
public sealed record SmsOutboxPayload(string PhoneNumber, string Body);

/// <summary>
/// Payload para mensagens de Push no outbox
/// </summary>
public sealed record PushOutboxPayload(string DeviceToken, string Title, string Body, IDictionary<string, string>? Data = null);
