using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Exceptions;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de correção de informações básicas.
/// </summary>
/// <remarks>
/// Este handler retorna o prestador da etapa de PendingDocumentVerification para PendingBasicInfo,
/// permitindo que o prestador corrija informações identificadas como incorretas ou incompletas
/// durante o processo de verificação de documentos.
/// </remarks>
/// <param name="providerRepository">Repositório para persistência de prestadores de serviços</param>
/// <param name="logger">Logger estruturado para auditoria e debugging</param>
public sealed class RequireBasicInfoCorrectionCommandHandler(
    IProviderRepository providerRepository,
    ILogger<RequireBasicInfoCorrectionCommandHandler> logger
) : ICommandHandler<RequireBasicInfoCorrectionCommand, Result>
{
    /// <summary>
    /// Processa o comando de solicitação de correção de informações básicas.
    /// </summary>
    /// <param name="command">Comando de solicitação de correção</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação</returns>
    public async Task<Result> HandleAsync(RequireBasicInfoCorrectionCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Requiring basic info correction for provider {ProviderId}. Reason: {Reason}, Requested by: {RequestedBy}",
                command.ProviderId, command.Reason, command.RequestedBy);

            if (string.IsNullOrWhiteSpace(command.Reason))
            {
                logger.LogWarning("Correction reason is required but was not provided");
                return Result.Failure("Correction reason is required");
            }

            if (string.IsNullOrWhiteSpace(command.RequestedBy))
            {
                logger.LogWarning("RequestedBy is required but was not provided");
                return Result.Failure("RequestedBy is required");
            }

            var provider = await providerRepository.GetByIdAsync(new ProviderId(command.ProviderId), cancellationToken);
            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result.Failure("Provider not found");
            }

            provider.RequireBasicInfoCorrection(command.Reason, command.RequestedBy);

            await providerRepository.UpdateAsync(provider, cancellationToken);

            logger.LogInformation(
                "Basic info correction required for provider {ProviderId}. Provider returned to PendingBasicInfo status",
                command.ProviderId);

            return Result.Success();
        }
        catch (ProviderDomainException ex)
        {
            // Preserve domain validation messages for actionable feedback
            logger.LogWarning(ex,
                "Domain validation failed when requiring basic info correction for provider {ProviderId}: {Message}",
                command.ProviderId, ex.Message);
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            // Generic error for unexpected failures
            logger.LogError(ex, "Error requiring basic info correction for provider {ProviderId}", command.ProviderId);
            return Result.Failure("Failed to require basic info correction");
        }
    }
}
