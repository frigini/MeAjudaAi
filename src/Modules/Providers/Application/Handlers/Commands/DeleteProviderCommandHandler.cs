using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Time;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de exclusão de prestadores de serviços.
/// </summary>
/// <param name="providerRepository">Repositório para acesso aos dados</param>
/// <param name="dateTimeProvider">Provedor de data/hora para auditoria</param>
/// <param name="logger">Logger estruturado</param>
public sealed class DeleteProviderCommandHandler(
    IProviderRepository providerRepository,
    IDateTimeProvider dateTimeProvider,
    ILogger<DeleteProviderCommandHandler> logger
) : ICommandHandler<DeleteProviderCommand, Result>
{
    /// <summary>
    /// Processa o comando de exclusão de prestador.
    /// </summary>
    public async Task<Result> HandleAsync(DeleteProviderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Deleting provider {ProviderId}", command.ProviderId);

            var providerId = new ProviderId(command.ProviderId);
            var provider = await providerRepository.GetByIdAsync(providerId, cancellationToken);

            if (provider == null)
            {
                logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
                return Result.Failure(Error.NotFound("Provider not found"));
            }

            provider.Delete(dateTimeProvider, command.DeletedBy);
            await providerRepository.UpdateAsync(provider, cancellationToken);

            logger.LogInformation("Provider {ProviderId} deleted successfully", command.ProviderId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting provider {ProviderId}", command.ProviderId);
            return Result.Failure("Error deleting provider");
        }
    }
}
