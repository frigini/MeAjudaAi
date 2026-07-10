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
/// Handler responsável por atualizar o device token do prestador de serviços.
/// </summary>
public sealed class UpdateProviderDeviceTokenCommandHandler(
    IUnitOfWork uow,
    ILogger<UpdateProviderDeviceTokenCommandHandler> logger,
    IStringLocalizer<Strings> localizer
) : ICommandHandler<UpdateProviderDeviceTokenCommand, Result>
{
    /// <summary>
    /// Processa a atualização do device token do prestador.
    /// </summary>
    /// <param name="command">Comando contendo o ID do prestador e o novo device token.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Result indicando sucesso ou falha da operação.</returns>
    public async Task<Result> HandleAsync(UpdateProviderDeviceTokenCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating device token for provider {ProviderId}", command.ProviderId);

        var providerId = new ProviderId(command.ProviderId);
        var provider = await uow.GetRepository<Provider, ProviderId>().TryFindAsync(providerId, cancellationToken);

        if (provider == null)
        {
            logger.LogWarning("Provider {ProviderId} not found for device token update.", command.ProviderId);
            return Result.Failure(Error.NotFound(localizer["ProviderNotFound"]));
        }

        var normalizedToken = string.IsNullOrWhiteSpace(command.DeviceToken) ? null : command.DeviceToken;
        var currentToken = string.IsNullOrWhiteSpace(provider.DeviceToken) ? null : provider.DeviceToken;

        if (normalizedToken == currentToken)
        {
            logger.LogInformation("Device token unchanged for provider {ProviderId}, skipping update.", command.ProviderId);
            return Result.Success();
        }

        provider.UpdateDeviceToken(command.DeviceToken);
        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Device token updated successfully for provider {ProviderId}.", command.ProviderId);
        return Result.Success();
    }
}