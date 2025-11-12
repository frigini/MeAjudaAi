using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de suspensão de prestadores.
/// </summary>
/// <remarks>
/// Este handler suspende temporariamente um prestador, impedindo que ele ofereça serviços
/// até que a suspensão seja revertida.
/// </remarks>
/// <param name="providerRepository">Repositório para persistência de prestadores de serviços</param>
/// <param name="logger">Logger estruturado para auditoria e debugging</param>
public sealed class SuspendProviderCommandHandler(
    IProviderRepository providerRepository,
    ILogger<SuspendProviderCommandHandler> logger
) : ICommandHandler<SuspendProviderCommand, Result>
{
    /// <summary>
    /// Processa o comando de suspensão de prestador.
    /// </summary>
    /// <param name="command">Comando de suspensão</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação</returns>
    public async Task<Result> HandleAsync(SuspendProviderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Suspending provider {ProviderId}. Reason: {Reason}", 
                command.ProviderId, command.Reason ?? "Not specified");

            var provider = await providerRepository.GetByIdAsync(new ProviderId(command.ProviderId), cancellationToken);
            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result.Failure("Provider not found");
            }

            provider.Suspend(command.SuspendedBy);

            await providerRepository.UpdateAsync(provider, cancellationToken);

            logger.LogInformation("Provider {ProviderId} suspended successfully", command.ProviderId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error suspending provider {ProviderId}", command.ProviderId);
            return Result.Failure(ex.Message);
        }
    }
}
