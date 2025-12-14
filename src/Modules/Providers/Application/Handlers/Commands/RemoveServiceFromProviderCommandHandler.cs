using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de remoção de serviços de providers.
/// </summary>
public sealed class RemoveServiceFromProviderCommandHandler(
    IProviderRepository providerRepository,
    ILogger<RemoveServiceFromProviderCommandHandler> logger
) : ICommandHandler<RemoveServiceFromProviderCommand, Result>
{
    /// <summary>
    /// Processa o comando de remoção de serviço do provider.
    /// </summary>
    /// <param name="command">Comando de remoção</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação</returns>
    public async Task<Result> HandleAsync(RemoveServiceFromProviderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Removing service {ServiceId} from provider {ProviderId}",
                command.ServiceId,
                command.ProviderId);

            // 1. Buscar o provider
            var provider = await providerRepository.GetByIdAsync(
                new ProviderId(command.ProviderId),
                cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result.Failure("Provider not found");
            }

            // 2. Remover o serviço do provider (domínio valida se existe)
            provider.RemoveService(command.ServiceId);

            // 3. Persistir mudanças
            await providerRepository.UpdateAsync(provider, cancellationToken);

            logger.LogInformation(
                "Service {ServiceId} successfully removed from provider {ProviderId}",
                command.ServiceId,
                command.ProviderId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error removing service {ServiceId} from provider {ProviderId}",
                command.ServiceId,
                command.ProviderId);

            return Result.Failure($"An error occurred while removing service from provider: {ex.Message}");
        }
    }
}
