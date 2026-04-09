using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Application.Services.Email;

/// <summary>
/// Stub para envio de e-mails em desenvolvimento/MVP.
/// Apenas loga no console as informações do e-mail.
/// </summary>
internal sealed class StubEmailService(ILogger<StubEmailService> logger) : IEmailService
{
    public async Task<Result<string>> SendAsync(EmailMessageDto message, CancellationToken cancellationToken = default)
    {
        // Simulação de delay de rede
        await Task.Delay(100, cancellationToken);

        logger.LogInformation(
            "[STUB EMAIL] Email dispatched (recipient and subject masked) | Body length: {BodyLength} bytes",
            message.Body.Length);

        // Retorna um ID falso do provedor
        return Result<string>.Success($"stub_{Guid.NewGuid():N}");
    }
}
