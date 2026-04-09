namespace MeAjudaAi.Contracts.Modules.Communications.DTOs;

/// <summary>
/// DTO para envio de mensagem SMS.
/// </summary>
public sealed record SmsMessageDto(
    string PhoneNumber,
    string Message
);
