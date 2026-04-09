using MeAjudaAi.Modules.Communications.Domain.Services;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Services;

/// <summary>
/// Stub para envio de notificações push na infraestrutura.
/// </summary>
internal sealed class PushSenderStub(ILogger<PushSenderStub> logger) : IPushSender
{
    public async Task<bool> SendAsync(PushNotification notification, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        logger.LogInformation("[STUB PUSH SENDER] Token: {Token} | Title: {Title}", notification.DeviceToken, notification.Title);
        return true;
    }
}
