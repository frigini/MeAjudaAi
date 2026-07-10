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
/// Handler responsável por processar comandos de remoção de serviços de providers.
/// </summary>
public sealed class RemoveServiceFromProviderCommandHandler(
    IUnitOfWork uow,
    ILogger<RemoveServiceFromProviderCommandHandler> logger,
    IStringLocalizer<Strings> localizer
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
        logger.LogInformation(
            "Removing service {ServiceId} from provider {ProviderId}",
            command.ServiceId,
            command.ProviderId);

        var provider = await uow.GetRepository<Provider, ProviderId>().TryFindAsync(
            new ProviderId(command.ProviderId),
            cancellationToken);

        if (provider == null)
        {
            logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
            return Result.Failure(Error.NotFound(localizer["ProviderNotFound"]));
        }

        provider.RemoveService(command.ServiceId);

        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Service {ServiceId} successfully removed from provider {ProviderId}",
            command.ServiceId,
            command.ProviderId);

        return Result.Success();
    }
}