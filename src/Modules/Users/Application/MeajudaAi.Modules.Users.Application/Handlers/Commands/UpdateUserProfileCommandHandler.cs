using MeAjudaAi.Modules.Users.Application.Caching;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Common;
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
internal sealed class UpdateUserProfileCommandHandler(
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
            // Busca o usuário pelo ID fornecido
            var user = await userRepository.GetByIdAsync(
                new UserId(command.UserId), cancellationToken);

            if (user == null)
            {
                logger.LogWarning("User profile update failed: User {UserId} not found", command.UserId);
                return Result<UserDto>.Failure("User not found");
            }

            logger.LogDebug("Updating profile for user {UserId}: FirstName={FirstName}, LastName={LastName}", 
                command.UserId, command.FirstName, command.LastName);

            // Atualiza o perfil através do método de domínio
            user.UpdateProfile(command.FirstName, command.LastName);

            // Persiste as alterações no repositório
            await userRepository.UpdateAsync(user, cancellationToken);

            // Invalida cache relacionado ao usuário atualizado
            await usersCacheService.InvalidateUserAsync(command.UserId, user.Email.Value, cancellationToken);

            logger.LogInformation("User profile updated successfully for user {UserId} - cache invalidated", command.UserId);

            return Result<UserDto>.Success(user.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error updating user profile for user {UserId}", command.UserId);
            return Result<UserDto>.Failure($"Failed to update user profile: {ex.Message}");
        }
    }
}