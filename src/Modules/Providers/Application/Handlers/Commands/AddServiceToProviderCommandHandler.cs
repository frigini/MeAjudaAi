using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de adição de serviços a providers.
/// Valida os serviços via IServiceCatalogsModuleApi antes de permitir a associação.
/// </summary>
public sealed class AddServiceToProviderCommandHandler(
    IProviderRepository providerRepository,
    IServiceCatalogsModuleApi serviceCatalogsModuleApi,
    ILogger<AddServiceToProviderCommandHandler> logger
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
        try
        {
            logger.LogInformation(
                "Adding service {ServiceId} to provider {ProviderId}",
                command.ServiceId,
                command.ProviderId);

            // 1. Buscar o provider
            var provider = await providerRepository.GetByIdAsync(
                new ProviderId(command.ProviderId),
                cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result.Failure("Prestador não encontrado");
            }

            // 2. Validar o serviço via IServiceCatalogsModuleApi
            var validationResult = await serviceCatalogsModuleApi.ValidateServicesAsync(
                new[] { command.ServiceId },
                cancellationToken);

            if (validationResult.IsFailure)
            {
                logger.LogWarning(
                    "Failed to validate service {ServiceId}: {Error}",
                    command.ServiceId,
                    validationResult.Error.Message);
                return Result.Failure($"Falha ao validar serviço: {validationResult.Error.Message}");
            }

            // 3. Verificar se o serviço é válido
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

            // 4. Adicionar o serviço ao provider (domínio valida duplicatas)
            provider.AddService(command.ServiceId);

            // 5. Persistir mudanças
            await providerRepository.UpdateAsync(provider, cancellationToken);

            logger.LogInformation(
                "Service {ServiceId} successfully added to provider {ProviderId}",
                command.ServiceId,
                command.ProviderId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error adding service {ServiceId} to provider {ProviderId}",
                command.ServiceId,
                command.ProviderId);

            return Result.Failure($"Ocorreu um erro ao adicionar serviço ao prestador: {ex.Message}");
        }
    }
}
