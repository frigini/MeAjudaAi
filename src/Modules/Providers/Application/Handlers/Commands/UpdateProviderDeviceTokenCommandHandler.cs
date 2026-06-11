using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

public sealed class UpdateProviderDeviceTokenCommandHandler(
    IProviderUnitOfWork uow,
    ILogger<UpdateProviderDeviceTokenCommandHandler> logger
) : ICommandHandler<UpdateProviderDeviceTokenCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateProviderDeviceTokenCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Updating device token for provider {ProviderId}", command.ProviderId);

            var providerId = new ProviderId(command.ProviderId);
            var provider = await uow.GetRepository<Provider, ProviderId>().TryFindAsync(providerId, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found for device token update.", command.ProviderId);
                return Result.Failure(Error.NotFound("Provider not found"));
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating device token for provider {ProviderId}", command.ProviderId);
            return Result.Failure("Error updating device token");
        }
    }
}
