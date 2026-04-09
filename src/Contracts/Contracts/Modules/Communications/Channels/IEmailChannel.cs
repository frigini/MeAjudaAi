using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using MeAjudaAi.Contracts.Functional;

namespace MeAjudaAi.Contracts.Modules.Communications.Channels;

/// <summary>
/// Interface para um canal de e-mail (SendGrid, Mailgun, SMTP, etc.).
/// </summary>
public interface IEmailChannel
{
    /// <summary>
    /// Nome descritivo do canal (ex: "SendGrid").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Envia uma mensagem de e-mail.
    /// </summary>
    /// <param name="message">DTO com dados do e-mail</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado com o ID da mensagem no provedor</returns>
    Task<Result<string>> SendAsync(EmailMessageDto message, CancellationToken cancellationToken = default);
}
