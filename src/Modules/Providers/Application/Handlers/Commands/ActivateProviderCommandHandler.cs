using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts.Modules;
using MeAjudaAi.Contracts.Modules.Documents;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Utilities.Constants;
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
                return Result.Failure(ValidationMessages.Providers.NotFound);
            }

            // Validar que provider tem documentos verificados via Documents module
            logger.LogDebug("Validating documents for provider {ProviderId} via IDocumentsModuleApi", command.ProviderId);

            var documentValidation = await ValidateDocumentConditionsAsync(command.ProviderId, cancellationToken);
            if (documentValidation.IsFailure)
            {
                logger.LogWarning("Provider {ProviderId} cannot be activated: {Error}",
                    command.ProviderId, documentValidation.Error);
                return documentValidation;
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
            return Result.Failure(ValidationMessages.Providers.ActivationFailed);
        }
    }

    /// <summary>
    /// Valida todas as condições de documentos necessárias para ativação do prestador.
    /// </summary>
    /// <param name="providerId">ID do prestador</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Result com sucesso ou falha baseada nas validações</returns>
    private async Task<Result> ValidateDocumentConditionsAsync(Guid providerId, CancellationToken cancellationToken)
    {
        // Valida que provider tem todos os documentos obrigatórios
        var hasRequiredDocsResult = await documentsModuleApi.HasRequiredDocumentsAsync(providerId, cancellationToken);
        var requiredDocsValidation = hasRequiredDocsResult.Match(
            hasRequired => hasRequired
                ? Result.Success()
                : Result.Failure(ValidationMessages.Providers.MustHaveAllDocuments),
            error => Result.Failure(ValidationMessages.Providers.ActivationFailed));

        if (requiredDocsValidation.IsFailure)
            return requiredDocsValidation;

        // Valida que provider tem documentos verificados
        var hasVerifiedDocsResult = await documentsModuleApi.HasVerifiedDocumentsAsync(providerId, cancellationToken);
        var verifiedDocsValidation = hasVerifiedDocsResult.Match(
            hasVerified => hasVerified
                ? Result.Success()
                : Result.Failure(ValidationMessages.Providers.MustHaveVerifiedDocuments),
            error => Result.Failure(ValidationMessages.Providers.ActivationFailed));

        if (verifiedDocsValidation.IsFailure)
            return verifiedDocsValidation;

        // Valida que provider não tem documentos pendentes
        var hasPendingDocsResult = await documentsModuleApi.HasPendingDocumentsAsync(providerId, cancellationToken);
        var pendingDocsValidation = hasPendingDocsResult.Match(
            hasPending => hasPending
                ? Result.Failure(ValidationMessages.Providers.CannotBeActivatedPendingDocs)
                : Result.Success(),
            error => Result.Failure(ValidationMessages.Providers.ActivationFailed));

        if (pendingDocsValidation.IsFailure)
            return pendingDocsValidation;

        // Valida que provider não tem documentos rejeitados
        var hasRejectedDocsResult = await documentsModuleApi.HasRejectedDocumentsAsync(providerId, cancellationToken);
        var rejectedDocsValidation = hasRejectedDocsResult.Match(
            hasRejected => hasRejected
                ? Result.Failure(ValidationMessages.Providers.CannotBeActivatedRejectedDocs)
                : Result.Success(),
            error => Result.Failure(ValidationMessages.Providers.ActivationFailed));

        return rejectedDocsValidation;
    }
}
