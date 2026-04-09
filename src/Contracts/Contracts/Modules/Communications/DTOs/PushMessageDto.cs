namespace MeAjudaAi.Contracts.Modules.Communications.DTOs;

/// <summary>
/// DTO para envio de notificação push.
/// </summary>
public sealed record PushMessageDto(
    string DeviceToken,
    string Title,
    string Body,
    IDictionary<string, string>? ExtraData = null
);
