using MeAjudaAi.Modules.Communications.Domain.Services;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Services;

/// <summary>
/// Stub para envio de e-mails na infraestrutura.
/// </summary>
internal sealed class EmailSenderStub(ILogger<EmailSenderStub> logger) : IEmailSender
{
    public async Task<bool> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        logger.LogInformation("[STUB EMAIL SENDER] To: {To} | Subject: {Subject}", message.To, message.Subject);
        return true;
    }
}
