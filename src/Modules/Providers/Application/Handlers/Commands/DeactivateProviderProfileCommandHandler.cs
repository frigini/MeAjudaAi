using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar o comando de desativação de perfil de prestador de serviços.
/// </summary>
public sealed class DeactivateProviderProfileCommandHandler(
    IUnitOfWork uow,
    ILogger<DeactivateProviderProfileCommandHandler> logger,
    IStringLocalizer<Strings> localizer
) : ICommandHandler<DeactivateProviderProfileCommand, Result>
{
    public async Task<Result> HandleAsync(DeactivateProviderProfileCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deactivating provider profile {ProviderId}", command.ProviderId);

        var providerId = new ProviderId(command.ProviderId);
        var provider = await uow.GetRepository<Provider, ProviderId>().TryFindAsync(providerId, cancellationToken);

        if (provider == null)
        {
            logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
            return Result.Failure(Error.NotFound(localizer["ProviderNotFound"]));
        }

        provider.DeactivateProfile(command.UpdatedBy);
        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Provider profile {ProviderId} deactivated successfully", command.ProviderId);
        return Result.Success();
    }
}