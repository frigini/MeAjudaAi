using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar o comando de ativação de perfil de prestador de serviços.
/// </summary>
public sealed class ActivateProviderProfileCommandHandler(
    IUnitOfWork uow,
    ILogger<ActivateProviderProfileCommandHandler> logger
) : ICommandHandler<ActivateProviderProfileCommand, Result>
{
    public async Task<Result> HandleAsync(ActivateProviderProfileCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Activating provider profile {ProviderId}", command.ProviderId);

        var providerId = new ProviderId(command.ProviderId);
        var provider = await uow.GetRepository<Provider, ProviderId>().TryFindAsync(providerId, cancellationToken);

        if (provider == null)
        {
            logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
            return Result.Failure(Error.NotFound(ValidationMessages.Providers.ProviderNotFound));
        }

        provider.ActivateProfile(command.UpdatedBy);
        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Provider profile {ProviderId} activated successfully", command.ProviderId);
        return Result.Success();
    }
}