using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

public sealed class DeleteProviderCommandHandler(
    IProviderUnitOfWork uow,
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
                return Result.Failure(Error.NotFound("Provider not found"));
            }

            provider.Delete(dateTimeProvider, command.DeletedBy);
            await uow.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Provider {ProviderId} deleted successfully", command.ProviderId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting provider {ProviderId}", command.ProviderId);
            return Result.Failure("Error deleting provider");
        }
    }
}
