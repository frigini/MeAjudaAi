namespace MeAjudaAi.Modules.Communications.Domain.Services;

/// <summary>
/// DTO que representa uma mensagem de e-mail a ser enviada.
/// </summary>
/// <param name="To">Endereço de destino.</param>
/// <param name="Subject">Assunto do e-mail.</param>
/// <param name="HtmlBody">Corpo HTML.</param>
/// <param name="TextBody">Corpo em texto puro.</param>
/// <param name="From">Endereço de origem (null usa padrão configurado).</param>
public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string TextBody,
    string? From = null
);

/// <summary>
/// Abstração do canal de envio de e-mail.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Envia um e-mail.
    /// </summary>
    /// <param name="message">Dados do e-mail a enviar.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>True se enviado com sucesso.</returns>
    Task<bool> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
