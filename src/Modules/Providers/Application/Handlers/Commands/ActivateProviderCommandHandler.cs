using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de ativação de prestadores.
/// </summary>
/// <remarks>
/// Este handler ativa um prestador após a verificação bem-sucedida dos documentos,
/// permitindo que ele comece a oferecer serviços na plataforma.
/// </remarks>
/// <param name="providerRepository">Repositório para persistência de prestadores de serviços</param>
/// <param name="logger">Logger estruturado para auditoria e debugging</param>
public sealed class ActivateProviderCommandHandler(
    IProviderRepository providerRepository,
    ILogger<ActivateProviderCommandHandler> logger
) : ICommandHandler<ActivateProviderCommand, Result>
{
    /// <summary>
    /// Processa o comando de ativação de prestador.
    /// </summary>
    /// <param name="command">Comando de ativação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação</returns>
    public async Task<Result> HandleAsync(ActivateProviderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Activating provider {ProviderId}", command.ProviderId);

            var provider = await providerRepository.GetByIdAsync(new ProviderId(command.ProviderId), cancellationToken);
            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result.Failure("Provider not found");
            }

            provider.Activate(command.ActivatedBy);

            await providerRepository.UpdateAsync(provider, cancellationToken);

            logger.LogInformation("Provider {ProviderId} activated successfully", command.ProviderId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error activating provider {ProviderId}", command.ProviderId);
            return Result.Failure(ex.Message);
        }
    }
}
