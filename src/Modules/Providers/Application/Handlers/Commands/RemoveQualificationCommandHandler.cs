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
/// Handler responsável por processar comandos de remoção de qualificações de prestadores de serviços.
/// </summary>
/// <param name="providerRepository">Repositório para acesso aos dados</param>
/// <param name="logger">Logger estruturado</param>
internal sealed class RemoveQualificationCommandHandler(
    IProviderRepository providerRepository,
    ILogger<RemoveQualificationCommandHandler> logger
) : ICommandHandler<RemoveQualificationCommand, Result<ProviderDto>>
{
    /// <summary>
    /// Processa o comando de remoção de qualificação.
    /// </summary>
    public async Task<Result<ProviderDto>> HandleAsync(RemoveQualificationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Removing qualification from provider {ProviderId}", command.ProviderId);

            var providerId = new ProviderId(command.ProviderId);
            var provider = await providerRepository.GetByIdAsync(providerId, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result<ProviderDto>.Failure("Provider not found");
            }

            provider.RemoveQualification(command.QualificationName);

            await providerRepository.UpdateAsync(provider, cancellationToken);

            logger.LogInformation("Qualification removed successfully from provider {ProviderId}", command.ProviderId);
            return Result<ProviderDto>.Success(provider.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing qualification from provider {ProviderId}", command.ProviderId);
            return Result<ProviderDto>.Failure("An error occurred while removing the qualification");
        }
    }
}
