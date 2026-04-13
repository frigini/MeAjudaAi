using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Contracts.Modules.Communications.Channels;

/// <summary>
/// Interface para um canal de SMS (Twilio, AWS Pinpoint, etc.).
/// </summary>
public interface ISmsChannel
{
    /// <summary>
    /// Nome descritivo do canal (ex: "Twilio").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Envia uma mensagem SMS.
    /// </summary>
    /// <param name="message">DTO com dados do SMS</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado com o ID da mensagem no provedor</returns>
    Task<Result<string>> SendAsync(SmsMessageDto message, CancellationToken cancellationToken = default);
}
