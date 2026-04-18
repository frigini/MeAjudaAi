using MeAjudaAi.Modules.Communications.Domain.Services;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Services;

/// <summary>
/// Stub para envio de notificaÃ§Ãµes push na infraestrutura.
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class PushSenderStub(ILogger<PushSenderStub> logger) : IPushSender
{
    public async Task<bool> SendAsync(PushNotification notification, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        logger.LogInformation("[STUB PUSH SENDER] Push notification sent (token masked) | Title: {Title}", notification.Title);
        return true;
    }
}

