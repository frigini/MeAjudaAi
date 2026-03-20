using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar o comando de ativação de perfil de prestador de serviços.
/// </summary>
public sealed class ActivateProviderProfileCommandHandler(
    IProviderRepository providerRepository,
    ILogger<ActivateProviderProfileCommandHandler> logger
) : ICommandHandler<ActivateProviderProfileCommand, Result>
{
    public async Task<Result> HandleAsync(ActivateProviderProfileCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Activating provider profile {ProviderId}", command.ProviderId);

            var providerId = new ProviderId(command.ProviderId);
            var provider = await providerRepository.GetByIdAsync(providerId, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result.Failure(Error.NotFound("Provider not found"));
            }

            provider.ActivateProfile(command.UpdatedBy);
            await providerRepository.UpdateAsync(provider, cancellationToken);

            logger.LogInformation("Provider profile {ProviderId} activated successfully", command.ProviderId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error activating provider profile {ProviderId}", command.ProviderId);
            return Result.Failure("Error activating provider profile");
        }
    }
}
