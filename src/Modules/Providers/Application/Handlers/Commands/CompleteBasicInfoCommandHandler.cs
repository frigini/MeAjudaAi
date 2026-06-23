using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de conclusão de informações básicas.
/// </summary>
/// <remarks>
/// Este handler move o prestador da etapa de PendingBasicInfo para PendingDocumentVerification,
/// indicando que as informações básicas foram preenchidas e o próximo passo é o envio de documentos.
/// </remarks>
/// <param name="uow">Unit of Work para persistência</param>
/// <param name="logger">Logger estruturado para auditoria e debugging</param>
public sealed class CompleteBasicInfoCommandHandler(
    IUnitOfWork uow,
    ILogger<CompleteBasicInfoCommandHandler> logger
) : ICommandHandler<CompleteBasicInfoCommand, Result>
{
    /// <summary>
    /// Processa o comando de conclusão de informações básicas.
    /// </summary>
    /// <param name="command">Comando de conclusão</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação</returns>
    public async Task<Result> HandleAsync(CompleteBasicInfoCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Completing basic info for provider {ProviderId}", command.ProviderId);

        var provider = await uow.GetRepository<Provider, ProviderId>().TryFindAsync(new ProviderId(command.ProviderId), cancellationToken);
        if (provider == null)
        {
            logger.LogWarning("Provider {ProviderId} not found", command.ProviderId);
            return Result.Failure("Provider not found");
        }

        provider.CompleteBasicInfo(command.UpdatedBy);

        await uow.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Basic info completed for provider {ProviderId}", command.ProviderId);
        return Result.Success();
    }
}