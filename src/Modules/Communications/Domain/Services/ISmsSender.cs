namespace MeAjudaAi.Modules.Communications.Domain.Services;

/// <summary>
/// DTO que representa uma mensagem SMS a ser enviada.
/// </summary>
/// <param name="PhoneNumber">Número de telefone no formato E.164 (+5511999999999).</param>
/// <param name="Body">Texto da mensagem.</param>
public sealed record SmsMessage(
    string PhoneNumber,
    string Body
);

/// <summary>
/// Abstração do canal de envio de SMS.
/// </summary>
public interface ISmsSender
{
    /// <summary>
    /// Envia uma mensagem SMS.
    /// </summary>
    /// <param name="message">Dados do SMS a enviar.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>True se enviado com sucesso.</returns>
    Task<bool> SendAsync(SmsMessage message, CancellationToken cancellationToken = default);
}
