using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar o comando de desativação de perfil de prestador de serviços.
/// </summary>
public sealed class DeactivateProviderProfileCommandHandler(
    IProviderRepository providerRepository,
    ILogger<DeactivateProviderProfileCommandHandler> logger
) : ICommandHandler<DeactivateProviderProfileCommand, Result>
{
    public async Task<Result> HandleAsync(DeactivateProviderProfileCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Deactivating provider profile {ProviderId}", command.ProviderId);

            var providerId = new ProviderId(command.ProviderId);
            var provider = await providerRepository.GetByIdAsync(providerId, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result.Failure(Error.NotFound("Provider not found"));
            }

            provider.DeactivateProfile(command.UpdatedBy);
            await providerRepository.UpdateAsync(provider, cancellationToken);

            logger.LogInformation("Provider profile {ProviderId} deactivated successfully", command.ProviderId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deactivating provider profile {ProviderId}", command.ProviderId);
            return Result.Failure("Error deactivating provider profile");
        }
    }
}
