using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de atualização do status de verificação de prestadores de serviços.
/// </summary>
/// <param name="providerRepository">Repositório para acesso aos dados</param>
/// <param name="logger">Logger estruturado</param>
internal sealed class UpdateVerificationStatusCommandHandler(
    IProviderRepository providerRepository,
    ILogger<UpdateVerificationStatusCommandHandler> logger
) : ICommandHandler<UpdateVerificationStatusCommand, Result<ProviderDto>>
{
    /// <summary>
    /// Processa o comando de atualização do status de verificação.
    /// </summary>
    public async Task<Result<ProviderDto>> HandleAsync(UpdateVerificationStatusCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Updating verification status for provider {ProviderId} to {Status}",
                command.ProviderId, command.Status);

            var providerId = new ProviderId(command.ProviderId);
            var provider = await providerRepository.GetByIdAsync(providerId, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result<ProviderDto>.Failure("Provider not found");
            }

            provider.UpdateVerificationStatus(command.Status, command.UpdatedBy);

            await providerRepository.UpdateAsync(provider, cancellationToken);

            logger.LogInformation("Verification status updated successfully for provider {ProviderId}", command.ProviderId);
            return Result<ProviderDto>.Success(provider.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating verification status for provider {ProviderId}", command.ProviderId);
            return Result<ProviderDto>.Failure("An error occurred while updating the verification status");
        }
    }
}
