using MeAjudaAi.Modules.Communications.Domain.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Communications.Infrastructure.Services;

/// <summary>
/// Valida se todos os serviços de comunicação obrigatórios estão registrados no startup.
/// Lança um erro claro se EnableStubs=false e nenhum provedor real estiver registrado.
/// </summary>
internal sealed class CommunicationsStartupValidator(
    bool stubsEnabled,
    IEmailSender? emailSender,
    ISmsSender? smsSender,
    IPushSender? pushSender,
    ILogger<CommunicationsStartupValidator> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var missingServices = new List<string>();

        if (emailSender == null)
            missingServices.Add(nameof(IEmailSender));
        if (smsSender == null)
            missingServices.Add(nameof(ISmsSender));
        if (pushSender == null)
            missingServices.Add(nameof(IPushSender));

        if (missingServices.Count > 0)
        {
            var missingList = string.Join(", ", missingServices);
            
            if (stubsEnabled)
            {
                // Stubs are enabled but somehow not registered - this shouldn't happen
                throw new InvalidOperationException(
                    $"Communications module: EnableStubs=true but stub services are not registered. " +
                    $"Missing: {missingList}. This indicates a bug in service registration.");
            }
            else
            {
                // Stubs disabled but no real providers registered
                throw new InvalidOperationException(
                    $"Communications module: EnableStubs=false but no real service providers are registered. " +
                    $"Missing: {missingList}. " +
                    $"Either register real providers (e.g., SendGrid for email, Twilio for SMS, Firebase for push) " +
                    $"or set 'Communications:EnableStubs=true' in configuration for development/testing.");
            }
        }

        logger.LogDebug("Communications startup validation passed. Stubs={StubsEnabled}", stubsEnabled);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
