using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Mappers;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Exceptions;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de remoção de qualificações de prestadores de serviços.
/// </summary>
/// <param name="uow">Unit of Work para persistência</param>
/// <param name="logger">Logger estruturado</param>
/// <param name="localizer">Localizador para mensagens de erro</param>
public sealed class RemoveQualificationCommandHandler(
    IUnitOfWork uow,
    ILogger<RemoveQualificationCommandHandler> logger,
    IStringLocalizer<Strings> localizer
) : ICommandHandler<RemoveQualificationCommand, Result<ProviderDto>>
{
    /// <summary>
    /// Processa o comando de remoção de qualificação.
    /// </summary>
    public async Task<Result<ProviderDto>> HandleAsync(RemoveQualificationCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Removing qualification from provider {ProviderId}", command.ProviderId);

        var providerId = new ProviderId(command.ProviderId);
        var provider = await uow.GetRepository<Provider, ProviderId>().TryFindAsync(providerId, cancellationToken);

        if (provider == null)
        {
            logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
            return Result<ProviderDto>.Failure(Error.NotFound(localizer["ProviderNotFound"]));
        }

        try 
        {
            provider.RemoveQualification(command.QualificationName);
        }
        catch (ProviderDomainException)
        {
            return Result<ProviderDto>.Failure(localizer["QualificationNotFound"]);
        }

        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Qualification removed successfully from provider {ProviderId}", command.ProviderId);
        return Result<ProviderDto>.Success(provider.ToDto());
    }
}