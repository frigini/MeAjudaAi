using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

public sealed class DeleteProviderCommandHandler(
    IUnitOfWork uow,
    TimeProvider dateTimeProvider,
    ILogger<DeleteProviderCommandHandler> logger
) : ICommandHandler<DeleteProviderCommand, Result>
{
    /// <summary>
    /// Processa o comando de exclusão de prestador.
    /// </summary>
    public async Task<Result> HandleAsync(DeleteProviderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Deleting provider {ProviderId}", command.ProviderId);

            var providerId = new ProviderId(command.ProviderId);
            var provider = await uow.GetRepository<Provider, ProviderId>().TryFindAsync(providerId, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result.Failure(Error.NotFound("Fornecedor não encontrado"));
            }

            provider.Delete(dateTimeProvider, command.DeletedBy);
            await uow.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Provider {ProviderId} deleted successfully", command.ProviderId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting provider {ProviderId}", command.ProviderId);
            return Result.Failure("Erro ao excluir fornecedor");
        }
    }
}