namespace MeAjudaAi.Modules.Communications.Application.DTOs;

/// <summary>
/// Payload para mensagens de Push no outbox.
/// </summary>
public sealed record PushOutboxPayload(string DeviceToken, string Title, string Body, IDictionary<string, string>? Data = null);