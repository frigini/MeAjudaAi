using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de rejeição de prestadores.
/// </summary>
/// <remarks>
/// Este handler rejeita o registro de um prestador após falha na verificação,
/// impedindo que ele seja ativado na plataforma.
/// </remarks>
/// <param name="providerRepository">Repositório para persistência de prestadores de serviços</param>
/// <param name="logger">Logger estruturado para auditoria e debugging</param>
public sealed class RejectProviderCommandHandler(
    IProviderRepository providerRepository,
    ILogger<RejectProviderCommandHandler> logger
) : ICommandHandler<RejectProviderCommand, Result>
{
    /// <summary>
    /// Processa o comando de rejeição de prestador.
    /// </summary>
    /// <param name="command">Comando de rejeição</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação</returns>
    public async Task<Result> HandleAsync(RejectProviderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Rejecting provider {ProviderId}. Reason: {Reason}", 
                command.ProviderId, command.Reason);

            if (string.IsNullOrWhiteSpace(command.Reason))
            {
                logger.LogWarning("Rejection reason is required but was not provided");
                return Result.Failure("Rejection reason is required");
            }

            if (string.IsNullOrWhiteSpace(command.RejectedBy))
            {
                logger.LogWarning("RejectedBy is required but was not provided");
                return Result.Failure("RejectedBy is required");
            }

            var provider = await providerRepository.GetByIdAsync(new ProviderId(command.ProviderId), cancellationToken);
            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result.Failure("Provider not found");
            }

            provider.Reject(command.Reason, command.RejectedBy);

            await providerRepository.UpdateAsync(provider, cancellationToken);

            logger.LogInformation("Provider {ProviderId} rejected successfully", command.ProviderId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rejecting provider {ProviderId}", command.ProviderId);
            return Result.Failure("Failed to reject provider");
        }
    }
}
