﻿using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Common;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de exclusão de usuários.
/// </summary>
/// <remarks>
/// Implementa o padrão CQRS para exclusão de usuários, incluindo sincronização
/// com Keycloak para desativação do usuário no sistema de autenticação externo.
/// Utiliza soft delete para manter histórico e integridade referencial.
/// </remarks>
/// <param name="userRepository">Repositório para persistência de usuários</param>
/// <param name="userDomainService">Serviço de domínio para operações complexas de usuário</param>
/// <param name="logger">Logger estruturado para auditoria e debugging</param>
internal sealed class DeleteUserCommandHandler(
    IUserRepository userRepository,
    IUserDomainService userDomainService,
    ILogger<DeleteUserCommandHandler> logger
) : ICommandHandler<DeleteUserCommand, Result>
{
    /// <summary>
    /// Processa o comando de exclusão de usuário de forma assíncrona.
    /// </summary>
    /// <param name="command">Comando contendo o ID do usuário a ser excluído</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado da operação indicando:
    /// - Sucesso: Usuário excluído com sucesso
    /// - Falha: Mensagem de erro descritiva
    /// </returns>
    /// <remarks>
    /// O processo inclui:
    /// 1. Busca do usuário por ID
    /// 2. Validação da existência do usuário
    /// 3. Sincronização com Keycloak para desativação
    /// 4. Soft delete ou hard delete no repositório local
    /// 
    /// Nota: Implementação atual usa hard delete, mas recomenda-se
    /// implementar soft delete para ambientes de produção.
    /// </remarks>
    public async Task<Result> HandleAsync(
        DeleteUserCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing DeleteUserCommand for user {UserId} with correlation {CorrelationId}", 
            command.UserId, command.CorrelationId);

        try
        {
            // Busca o usuário pelo ID fornecido
            var user = await userRepository.GetByIdAsync(
                new UserId(command.UserId), cancellationToken);

            if (user == null)
            {
                logger.LogWarning("User deletion failed: User {UserId} not found", command.UserId);
                return Result.Failure(Error.NotFound("User not found"));
            }

            logger.LogDebug("Found user {UserId}, proceeding with deletion process", command.UserId);

            // Desativa primeiro no Keycloak para manter consistência
            var syncResult = await userDomainService.SyncUserWithKeycloakAsync(
                user.Id, cancellationToken);

            if (syncResult.IsFailure)
            {
                logger.LogError("Keycloak sync failed for user {UserId}: {Error}", command.UserId, syncResult.Error);
                return syncResult;
            }

            logger.LogDebug("Keycloak sync completed for user {UserId}, proceeding with database deletion", command.UserId);

            // Exclusão lógica no banco de dados local
            user.MarkAsDeleted();
            await userRepository.UpdateAsync(user, cancellationToken);

            logger.LogInformation("User {UserId} marked as deleted successfully", command.UserId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error deleting user {UserId}", command.UserId);
            return Result.Failure($"Failed to delete user: {ex.Message}");
        }
    }
}