using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Exceptions;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de suspensão de prestadores.
/// </summary>
/// <remarks>
/// Este handler suspende temporariamente um prestador, impedindo que ele ofereça serviços
/// até que a suspensão seja revertida.
/// </remarks>
/// <param name="uow">Unit of Work para persistência</param>
/// <param name="logger">Logger estruturado para auditoria e debugging</param>
public sealed class SuspendProviderCommandHandler(
    IUnitOfWork uow,
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
                command.ProviderId, command.Reason);

            if (string.IsNullOrWhiteSpace(command.Reason))
            {
                logger.LogWarning("Suspension reason is required but was not provided");
                return Result.Failure("Suspension reason is required");
            }

            if (string.IsNullOrWhiteSpace(command.SuspendedBy))
            {
                logger.LogWarning("SuspendedBy is required but was not provided");
                return Result.Failure("SuspendedBy is required");
            }

            var provider = await uow.GetRepository<Provider, ProviderId>().TryFindAsync(new ProviderId(command.ProviderId), cancellationToken);
            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result.Failure("Provider not found");
            }

            provider.Suspend(command.Reason, command.SuspendedBy);

            await uow.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Provider {ProviderId} suspended successfully", command.ProviderId);
            return Result.Success();
        }
        catch (ProviderDomainException ex)
        {
            logger.LogWarning(ex, "Domain validation failed while suspending provider {ProviderId}", command.ProviderId);
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error suspending provider {ProviderId}", command.ProviderId);
            return Result.Failure("Failed to suspend provider");
        }
    }
}