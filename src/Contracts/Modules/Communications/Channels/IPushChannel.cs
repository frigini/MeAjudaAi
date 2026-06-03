using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Contracts.Modules.Communications.Channels;

/// <summary>
/// Interface para um canal de push notification (Firebase, OneSignal, etc.).
/// </summary>
public interface IPushChannel
{
    /// <summary>
    /// Nome descritivo do canal (ex: "Firebase").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Envia uma notificação push.
    /// </summary>
    /// <param name="message">DTO com dados do push</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado com o ID da mensagem no provedor</returns>
    Task<Result<string>> SendAsync(PushMessageDto message, CancellationToken cancellationToken = default);
}
