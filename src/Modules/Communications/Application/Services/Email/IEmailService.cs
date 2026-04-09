using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Communications.DTOs;

namespace MeAjudaAi.Modules.Communications.Application.Services.Email;

/// <summary>
/// Serviço interno para envio de e-mails, abstraindo o canal real.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envia uma mensagem de e-mail de forma imediata.
    /// </summary>
    /// <param name="message">DTO com dados do e-mail</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>ID da mensagem no provedor</returns>
    Task<Result<string>> SendAsync(EmailMessageDto message, CancellationToken cancellationToken = default);
}
