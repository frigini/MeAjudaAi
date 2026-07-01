using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de adição de serviços a providers.
/// Valida os serviços via IServiceCatalogsModuleApi antes de permitir a associação.
/// </summary>
public sealed class AddServiceToProviderCommandHandler(
    IUnitOfWork uow,
    IServiceCatalogsModuleApi serviceCatalogsModuleApi,
    ILogger<AddServiceToProviderCommandHandler> logger,
    IStringLocalizer<Strings> localizer
) : ICommandHandler<AddServiceToProviderCommand, Result>
{
    /// <summary>
    /// Processa o comando de adição de serviço ao provider.
    /// </summary>
    /// <param name="command">Comando de adição</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação</returns>
    public async Task<Result> HandleAsync(AddServiceToProviderCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Adding service {ServiceId} to provider {ProviderId}",
            command.ServiceId,
            command.ProviderId);

        var provider = await uow.GetRepository<Provider, ProviderId>().TryFindAsync(
            new ProviderId(command.ProviderId),
            cancellationToken);

        if (provider == null)
        {
            logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
            return Result.Failure(Error.NotFound(localizer["ProviderNotFound"]));
        }

        var validationResult = await serviceCatalogsModuleApi.ValidateServicesAsync(
            new[] { command.ServiceId },
            cancellationToken);

        if (validationResult.IsFailure || validationResult.Value is null)
        {
            logger.LogWarning(
                "Failed to validate service {ServiceId}: {Error}",
                command.ServiceId,
                validationResult.Error?.Message ?? "Unknown error");
            return Result.Failure($"Falha ao validar serviço: {validationResult.Error?.Message ?? "erro desconhecido"}");
        }

        if (!validationResult.Value.AllValid)
        {
            var reasons = new List<string>();

            if (validationResult.Value.InvalidServiceIds.Any())
            {
                reasons.Add($"Serviço {command.ServiceId} não existe");
            }

            if (validationResult.Value.InactiveServiceIds.Any())
            {
                reasons.Add($"Serviço {command.ServiceId} não está ativo");
            }

            var errorMessage = string.Join("; ", reasons);
            logger.LogWarning(
                "Service {ServiceId} validation failed: {Reasons}",
                command.ServiceId,
                errorMessage);

            return Result.Failure(errorMessage);
        }

        var serviceResult = await serviceCatalogsModuleApi.GetServiceByIdAsync(
            command.ServiceId,
            cancellationToken);

        if (serviceResult.IsFailure || serviceResult.Value is null)
        {
            logger.LogError("Service {ServiceId} validated but details could not be retrieved", command.ServiceId);
            return Result.Failure("Falha ao recuperar detalhes do serviço.");
        }

        var serviceName = serviceResult.Value.Name;

        provider.AddService(command.ServiceId, serviceName);

        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Service {ServiceId} successfully added to provider {ProviderId}",
            command.ServiceId,
            command.ProviderId);

        return Result.Success();
    }
}