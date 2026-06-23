using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de atualização do status de verificação de prestadores de serviços.
/// </summary>
/// <param name="uow">Unit of Work para persistência</param>
/// <param name="logger">Logger estruturado</param>
/// <param name="localizer">Localizador de strings</param>
public sealed class UpdateVerificationStatusCommandHandler(
    IUnitOfWork uow,
    ILogger<UpdateVerificationStatusCommandHandler> logger,
    IStringLocalizer<Strings> localizer
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
            var provider = await uow.GetRepository<Provider, ProviderId>().TryFindAsync(providerId, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result<ProviderDto>.Failure(localizer["ProviderNotFound"]);
            }

            provider.UpdateVerificationStatus(command.Status, command.UpdatedBy);

            await uow.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Verification status updated successfully for provider {ProviderId}", command.ProviderId);
            return Result<ProviderDto>.Success(provider.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating verification status for provider {ProviderId}", command.ProviderId);
            return Result<ProviderDto>.Failure(localizer["VerificationStatusUpdateError"]);
        }
    }
}