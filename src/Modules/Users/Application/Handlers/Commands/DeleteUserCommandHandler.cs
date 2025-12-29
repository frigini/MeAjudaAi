using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.Services.Interfaces;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Shared.Functional;
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
/// <param name="usersCacheService">Serviço de cache para invalidação</param>
/// <param name="dateTimeProvider">Provedor de data/hora para testabilidade</param>
/// <param name="logger">Logger estruturado para auditoria e debugging</param>
internal sealed class DeleteUserCommandHandler(
    IUserRepository userRepository,
    IUserDomainService userDomainService,
    IUsersCacheService usersCacheService,
    TimeProvider dateTimeProvider,
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
            // Buscar e validar usuário
            var userResult = await GetAndValidateUserAsync(command, cancellationToken);
            if (userResult.IsFailure)
                return Result.Failure(userResult.Error);

            var user = userResult.Value;

            // Sincronizar com Keycloak
            var syncResult = await SyncWithKeycloakAsync(user, cancellationToken);
            if (syncResult.IsFailure)
                return syncResult;

            // Aplicar exclusão e persistir
            await ApplyDeletionAndPersistAsync(user, cancellationToken);

            // Invalidate cache
            await usersCacheService.InvalidateUserAsync(command.UserId, user.Email.Value, cancellationToken);

            logger.LogInformation("User {UserId} marked as deleted successfully", command.UserId);
            return Result.Success();
        }
        catch (ArgumentException)
        {
            // Allow ArgumentException (validation errors) to propagate to GlobalExceptionHandler
            throw;
        }
        catch (MeAjudaAi.Shared.Exceptions.ValidationException)
        {
            // Allow ValidationException to propagate to GlobalExceptionHandler
            throw;
        }
        catch (MeAjudaAi.Shared.Exceptions.DomainException)
        {
            // Allow DomainException (business rule violations) to propagate to GlobalExceptionHandler
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error deleting user {UserId}", command.UserId);
            return Result.Failure($"Failed to delete user: {ex.Message}");
        }
    }

    /// <summary>
    /// Busca e valida a existência do usuário.
    /// </summary>
    private async Task<Result<Domain.Entities.User>> GetAndValidateUserAsync(
        DeleteUserCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Fetching user {UserId} for deletion", command.UserId);

        var user = await userRepository.GetByIdAsync(
            new UserId(command.UserId), cancellationToken);

        if (user == null)
        {
            logger.LogWarning("User deletion failed: User {UserId} not found", command.UserId);
            return Result<Domain.Entities.User>.Failure(Error.NotFound(ValidationMessages.NotFound.User));
        }

        logger.LogDebug("Found user {UserId}, proceeding with deletion process", command.UserId);
        return Result<Domain.Entities.User>.Success(user);
    }

    /// <summary>
    /// Sincroniza a exclusão do usuário com o Keycloak.
    /// </summary>
    private async Task<Result> SyncWithKeycloakAsync(
        Domain.Entities.User user,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Starting Keycloak sync for user {UserId}", user.Id);

        var syncResult = await userDomainService.SyncUserWithKeycloakAsync(
            user.Id, cancellationToken);

        if (syncResult.IsFailure)
        {
            logger.LogError("Keycloak sync failed for user {UserId}: {Error}", user.Id, syncResult.Error);
            return syncResult;
        }

        logger.LogDebug("Keycloak sync completed for user {UserId}, proceeding with database deletion", user.Id);
        return Result.Success();
    }

    /// <summary>
    /// Aplica a exclusão lógica e persiste as alterações.
    /// </summary>
    private async Task ApplyDeletionAndPersistAsync(
        Domain.Entities.User user,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Applying logical deletion for user {UserId}", user.Id);

        user.MarkAsDeleted(dateTimeProvider);
        await userRepository.UpdateAsync(user, cancellationToken);

        logger.LogDebug("User {UserId} deletion persisted successfully", user.Id);
    }
}
