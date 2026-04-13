using MeAjudaAi.Modules.Communications.Domain.Services;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Services;

/// <summary>
/// Stub para envio de SMS na infraestrutura.
/// </summary>
internal sealed class SmsSenderStub(ILogger<SmsSenderStub> logger) : ISmsSender
{
    public async Task<bool> SendAsync(SmsMessage message, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        logger.LogInformation("[STUB SMS SENDER] SMS sent successfully (number and message redacted).");
        return true;
    }
}
