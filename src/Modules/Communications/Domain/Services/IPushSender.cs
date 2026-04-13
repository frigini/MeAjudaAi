namespace MeAjudaAi.Modules.Communications.Domain.Services;

/// <summary>
/// DTO que representa uma notificação push a ser enviada.
/// </summary>
/// <param name="DeviceToken">Token do dispositivo destino.</param>
/// <param name="Title">Título da notificação.</param>
/// <param name="Body">Corpo da notificação.</param>
/// <param name="Data">Dados adicionais opcionais (key-value pairs).</param>
public sealed record PushNotification(
    string DeviceToken,
    string Title,
    string Body,
    IDictionary<string, string>? Data = null
);

/// <summary>
/// Abstração do canal de envio de notificações push.
/// </summary>
public interface IPushSender
{
    /// <summary>
    /// Envia uma notificação push.
    /// </summary>
    /// <param name="notification">Dados da notificação a enviar.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>True se enviada com sucesso.</returns>
    Task<bool> SendAsync(PushNotification notification, CancellationToken cancellationToken = default);
}
