using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Application.Services.Interfaces;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de atualização de perfil de usuários.
/// </summary>
/// <remarks>
/// Implementa o padrão CQRS para atualização de dados do perfil do usuário,
/// utilizando o método de domínio UpdateProfile para garantir consistência
/// e validações de negócio. Opera diretamente no agregado User.
/// Invalida cache automaticamente após atualizações.
/// </remarks>
/// <param name="userRepository">Repositório para persistência de usuários</param>
/// <param name="usersCacheService">Serviço de cache para invalidação</param>
/// <param name="logger">Logger estruturado para auditoria e debugging</param>
public sealed class UpdateUserProfileCommandHandler(
    IUserRepository userRepository,
    IUsersCacheService usersCacheService,
    ILogger<UpdateUserProfileCommandHandler> logger
) : ICommandHandler<UpdateUserProfileCommand, Result<UserDto>>
{
    /// <summary>
    /// Processa o comando de atualização de perfil de usuário de forma assíncrona.
    /// </summary>
    /// <param name="command">Comando contendo o ID do usuário e novos dados do perfil</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado da operação contendo:
    /// - Sucesso: UserDto com os dados atualizados do usuário
    /// - Falha: Mensagem de erro caso o usuário não seja encontrado
    /// </returns>
    /// <remarks>
    /// O processo inclui:
    /// 1. Busca do usuário por ID
    /// 2. Validação da existência do usuário
    /// 3. Atualização do perfil através do método de domínio
    /// 4. Persistência das alterações
    /// 5. Retorno do DTO atualizado
    /// </remarks>
    public async Task<Result<UserDto>> HandleAsync(
        UpdateUserProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing UpdateUserProfileCommand for user {UserId} with correlation {CorrelationId}",
            command.UserId, command.CorrelationId);

        try
        {
            // Buscar e validar usuário
            var userResult = await GetAndValidateUserAsync(command, cancellationToken);
            if (userResult.IsFailure)
                return Result<UserDto>.Failure(userResult.Error);

            var user = userResult.Value;

            // Aplicar atualização do perfil
            ApplyProfileUpdate(command, user);

            // Persistir alterações e invalidar cache
            await PersistAndInvalidateCacheAsync(command, user, cancellationToken);

            logger.LogInformation("User profile updated successfully for user {UserId} - cache invalidated", command.UserId);
            return Result<UserDto>.Success(user.ToDto());
        }
        catch (ArgumentException)
        {
            // Allow ArgumentException (validation errors) to propagate to GlobalExceptionHandler
            throw;
        }
        catch (Exception ex)
        {
            // Catch infrastructure errors (database, cache, etc.) and return failure result
            logger.LogError(ex, "Unexpected error updating user profile for {UserId}", command.UserId);
            return Result<UserDto>.Failure("Failed to update user profile due to an unexpected error");
        }
    }

    /// <summary>
    /// Busca e valida a existência do usuário.
    /// </summary>
    private async Task<Result<Domain.Entities.User>> GetAndValidateUserAsync(
        UpdateUserProfileCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Fetching user {UserId} for profile update", command.UserId);

        var user = await userRepository.GetByIdAsync(
            new UserId(command.UserId), cancellationToken);

        if (user == null)
        {
            logger.LogWarning("User profile update failed: User {UserId} not found", command.UserId);
            return Result<Domain.Entities.User>.Failure(Error.NotFound(ValidationMessages.NotFound.User));
        }

        return Result<Domain.Entities.User>.Success(user);
    }

    /// <summary>
    /// Aplica a atualização do perfil usando o método de domínio.
    /// </summary>
    private void ApplyProfileUpdate(UpdateUserProfileCommand command, Domain.Entities.User user)
    {
        logger.LogDebug("Updating profile for user {UserId}: FirstName={FirstName}, LastName={LastName}",
            command.UserId, command.FirstName, command.LastName);

        user.UpdateProfile(command.FirstName, command.LastName);
    }

    /// <summary>
    /// Persiste as alterações e invalida o cache relacionado.
    /// </summary>
    private async Task PersistAndInvalidateCacheAsync(
        UpdateUserProfileCommand command,
        Domain.Entities.User user,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Persisting profile changes for user {UserId}", command.UserId);

        // Persiste as alterações no repositório
        await userRepository.UpdateAsync(user, cancellationToken);

        // Invalida cache relacionado ao usuário atualizado
        await usersCacheService.InvalidateUserAsync(command.UserId, user.Email.Value, cancellationToken);

        logger.LogDebug("Profile persistence and cache invalidation completed for user {UserId}", command.UserId);
    }
}
