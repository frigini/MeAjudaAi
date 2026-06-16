namespace MeAjudaAi.Modules.Communications.Application.DTOs;

/// <summary>
/// Payload para mensagens de SMS no outbox.
/// </summary>
public sealed record SmsOutboxPayload(string PhoneNumber, string Body);