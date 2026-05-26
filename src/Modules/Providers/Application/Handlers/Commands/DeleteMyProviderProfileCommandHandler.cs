using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de exclusão de perfil pelo próprio prestador de serviços.
/// </summary>
public sealed class DeleteMyProviderProfileCommandHandler(
    IUnitOfWork uow,
    TimeProvider dateTimeProvider,
    ILogger<DeleteMyProviderProfileCommandHandler> logger
) : ICommandHandler<DeleteMyProviderProfileCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteMyProviderProfileCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Provider self-deleting profile {ProviderId}", command.ProviderId);

            var providerId = new ProviderId(command.ProviderId);
            var provider = await uow.GetRepository<Provider, ProviderId>().TryFindAsync(providerId, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result.Failure(Error.NotFound("Prestador não encontrado"));
            }

            provider.Delete(dateTimeProvider, command.DeletedBy);
            await uow.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Provider profile {ProviderId} self-deleted successfully", command.ProviderId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao excluir o perfil do prestador {ProviderId}", command.ProviderId);
            return Result.Failure("Erro ao excluir o perfil do prestador");
        }
    }
}
