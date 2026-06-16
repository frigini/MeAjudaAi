using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de rejeição de prestadores.
/// </summary>
public sealed class RejectProviderCommandHandler(
    IUnitOfWork uow,
    ILogger<RejectProviderCommandHandler> logger
) : ICommandHandler<RejectProviderCommand, Result>
{
    public async Task<Result> HandleAsync(RejectProviderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Rejecting provider {ProviderId}. Reason: {Reason}",
                command.ProviderId, command.Reason);

            if (string.IsNullOrWhiteSpace(command.Reason))
            {
                logger.LogWarning("Rejection reason is required but was not provided");
                return Result.Failure("Motivo da rejeição é obrigatório");
            }

            if (string.IsNullOrWhiteSpace(command.RejectedBy))
            {
                logger.LogWarning("RejectedBy is required but was not provided");
                return Result.Failure("Responsável pela rejeição é obrigatório");
            }

            var provider = await uow.GetRepository<Provider, ProviderId>().TryFindAsync(new ProviderId(command.ProviderId), cancellationToken);
            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result.Failure("Fornecedor não encontrado");
            }

            provider.Reject(command.Reason, command.RejectedBy);

            await uow.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Provider {ProviderId} rejected successfully", command.ProviderId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rejecting provider {ProviderId}", command.ProviderId);
            return Result.Failure("Falha ao rejeitar o fornecedor");
        }
    }
}


