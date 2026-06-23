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
/// Handler responsável por processar comandos de adição de qualificações a prestadores de serviços.
/// </summary>
/// <param name="uow">Unit of Work para persistência</param>
/// <param name="logger">Logger estruturado</param>
/// <param name="localizer">Localizador de strings</param>
public sealed class AddQualificationCommandHandler(
    IUnitOfWork uow,
    ILogger<AddQualificationCommandHandler> logger,
    IStringLocalizer<Strings> localizer
) : ICommandHandler<AddQualificationCommand, Result<ProviderDto>>
{
    /// <summary>
    /// Processa o comando de adição de qualificação.
    /// </summary>
    public async Task<Result<ProviderDto>> HandleAsync(AddQualificationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Adding qualification to provider {ProviderId}", command.ProviderId);

            var providerId = new ProviderId(command.ProviderId);
            var provider = await uow.GetRepository<Provider, ProviderId>().TryFindAsync(providerId, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result<ProviderDto>.Failure(localizer["ProviderNotFound"]);
            }

            var qualification = new Qualification(
                command.Name,
                command.Description,
                command.IssuingOrganization,
                command.IssueDate,
                command.ExpirationDate,
                command.DocumentNumber
            );

            provider.AddQualification(qualification);

            await uow.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Qualification added successfully to provider {ProviderId}", command.ProviderId);
            return Result<ProviderDto>.Success(provider.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding qualification to provider {ProviderId}", command.ProviderId);
            return Result<ProviderDto>.Failure(localizer["QualificationAddError"]);
        }
    }
}