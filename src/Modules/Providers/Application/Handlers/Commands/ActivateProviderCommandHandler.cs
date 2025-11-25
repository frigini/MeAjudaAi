using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Contracts.Modules;
using MeAjudaAi.Shared.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de ativação de prestadores.
/// </summary>
/// <remarks>
/// Este handler ativa um prestador após a verificação bem-sucedida dos documentos,
/// permitindo que ele comece a oferecer serviços na plataforma.
/// Integra com Documents module para validar que prestador possui documentos verificados.
/// </remarks>
/// <param name="providerRepository">Repositório para persistência de prestadores de serviços</param>
/// <param name="documentsModuleApi">API do módulo Documents para validação de documentos</param>
/// <param name="logger">Logger estruturado para auditoria e debugging</param>
public sealed class ActivateProviderCommandHandler(
    IProviderRepository providerRepository,
    IDocumentsModuleApi documentsModuleApi,
    ILogger<ActivateProviderCommandHandler> logger
) : ICommandHandler<ActivateProviderCommand, Result>
{
    /// <summary>
    /// Processa o comando de ativação de prestador.
    /// </summary>
    /// <param name="command">Comando de ativação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação</returns>
    public async Task<Result> HandleAsync(ActivateProviderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Activating provider {ProviderId}", command.ProviderId);

            var provider = await providerRepository.GetByIdAsync(new ProviderId(command.ProviderId), cancellationToken);
            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result.Failure("Provider not found");
            }

            // Validar que provider tem documentos verificados via Documents module
            logger.LogDebug("Validating documents for provider {ProviderId} via IDocumentsModuleApi", command.ProviderId);
            var hasRequiredDocsResult = await documentsModuleApi.HasRequiredDocumentsAsync(command.ProviderId, cancellationToken);
            if (hasRequiredDocsResult.IsFailure)
            {
                logger.LogError("Failed to check required documents for provider {ProviderId}: {Error}",
                    command.ProviderId, hasRequiredDocsResult.Error);
                return Result.Failure($"Failed to validate documents: {hasRequiredDocsResult.Error}");
            }

            if (!hasRequiredDocsResult.Value)
            {
                logger.LogWarning("Provider {ProviderId} cannot be activated: missing required documents", command.ProviderId);
                return Result.Failure("Provider must have all required documents before activation");
            }

            var hasVerifiedDocsResult = await documentsModuleApi.HasVerifiedDocumentsAsync(command.ProviderId, cancellationToken);
            if (hasVerifiedDocsResult.IsFailure)
            {
                logger.LogError("Failed to check verified documents for provider {ProviderId}: {Error}",
                    command.ProviderId, hasVerifiedDocsResult.Error);
                return Result.Failure($"Failed to validate documents: {hasVerifiedDocsResult.Error}");
            }

            if (!hasVerifiedDocsResult.Value)
            {
                logger.LogWarning("Provider {ProviderId} cannot be activated: no verified documents", command.ProviderId);
                return Result.Failure("Provider must have verified documents before activation");
            }

            var hasPendingDocsResult = await documentsModuleApi.HasPendingDocumentsAsync(command.ProviderId, cancellationToken);
            if (hasPendingDocsResult.IsFailure)
            {
                logger.LogError("Failed to check pending documents for provider {ProviderId}: {Error}",
                    command.ProviderId, hasPendingDocsResult.Error);
                return Result.Failure($"Failed to validate documents: {hasPendingDocsResult.Error}");
            }

            if (hasPendingDocsResult.Value)
            {
                logger.LogWarning("Provider {ProviderId} cannot be activated: has pending documents", command.ProviderId);
                return Result.Failure("Provider cannot be activated while documents are pending verification");
            }

            var hasRejectedDocsResult = await documentsModuleApi.HasRejectedDocumentsAsync(command.ProviderId, cancellationToken);
            if (hasRejectedDocsResult.IsFailure)
            {
                logger.LogError("Failed to check rejected documents for provider {ProviderId}: {Error}",
                    command.ProviderId, hasRejectedDocsResult.Error);
                return Result.Failure($"Failed to validate documents: {hasRejectedDocsResult.Error}");
            }

            if (hasRejectedDocsResult.Value)
            {
                logger.LogWarning("Provider {ProviderId} cannot be activated: has rejected documents", command.ProviderId);
                return Result.Failure("Provider cannot be activated with rejected documents. Please resubmit correct documents.");
            }

            logger.LogInformation("Provider {ProviderId} passed all document validations", command.ProviderId);

            provider.Activate(command.ActivatedBy);

            await providerRepository.UpdateAsync(provider, cancellationToken);

            logger.LogInformation("Provider {ProviderId} activated successfully", command.ProviderId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error activating provider {ProviderId}", command.ProviderId);
            return Result.Failure("Failed to activate provider");
        }
    }
}
