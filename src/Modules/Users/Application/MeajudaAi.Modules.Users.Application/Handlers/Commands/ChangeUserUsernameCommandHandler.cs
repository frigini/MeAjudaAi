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
/// Handler responsável por processar comandos de alteração de username de usuários.
/// </summary>
/// <remarks>
/// **Operação de Identidade Crítica:**
/// Este handler processa alterações de username que impactam a identidade
/// pública do usuário e podem afetar URLs, menções e referências externas.
/// 
/// **Responsabilidades:**
/// - Validação de existência do usuário
/// - Verificação de unicidade do novo username
/// - Aplicação de regras de formato e negócio
/// - Controle de rate limiting para mudanças frequentes
/// - Logging detalhado para auditoria
/// - Sincronização com sistemas externos
/// 
/// **Considerações de Negócio:**
/// - Username alterado pode quebrar URLs existentes
/// - Histórico de menções pode ser afetado
/// - SEO e links externos podem ser impactados
/// - Possível necessidade de período de carência entre mudanças
/// </remarks>
/// <param name="userRepository">Repositório para operações de usuário</param>
/// <param name="logger">Logger estruturado para auditoria detalhada</param>
internal sealed class ChangeUserUsernameCommandHandler(
    IUserRepository userRepository,
    ILogger<ChangeUserUsernameCommandHandler> logger
) : ICommandHandler<ChangeUserUsernameCommand, Result<UserDto>>
{
    /// <summary>
    /// Processa o comando de alteração de username de forma assíncrona.
    /// </summary>
    /// <param name="command">Comando contendo ID do usuário e novo username</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado da operação contendo:
    /// - Sucesso: UserDto com username atualizado
    /// - Falha: Mensagem descritiva do erro
    /// </returns>
    /// <remarks>
    /// **Fluxo de Validação:**
    /// 1. ✅ Validação de existência do usuário
    /// 2. ✅ Verificação de unicidade do username
    /// 3. ✅ Validação de formato e tamanho
    /// 4. ✅ Controle de rate limiting (se aplicável)
    /// 5. ✅ Aplicação de regras de domínio
    /// 6. ✅ Logging detalhado para auditoria
    /// 7. ✅ Persistência atomica das alterações
    /// 
    /// **Validações Automáticas:**
    /// - Formato válido (letras, números, pontos, hífens, underscores)
    /// - Tamanho entre 3 e 50 caracteres
    /// - Username único no sistema
    /// - Usuário não deletado
    /// </remarks>
    public async Task<Result<UserDto>> HandleAsync(
        ChangeUserUsernameCommand command,
        CancellationToken cancellationToken = default)
    {
        using var activity = logger.BeginScope(new Dictionary<string, object>
        {
            ["UserId"] = command.UserId,
            ["NewUsername"] = command.NewUsername,
            ["UpdatedBy"] = command.UpdatedBy ?? "Unknown",
            ["BypassRateLimit"] = command.BypassRateLimit,
            ["Operation"] = "ChangeUsername"
        });

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        logger.LogInformation("Starting username change process for user {UserId} to {NewUsername}", 
            command.UserId, command.NewUsername);

        try
        {
            // Busca o usuário pelo ID
            logger.LogDebug("Fetching user {UserId} for username change", command.UserId);
            var user = await userRepository.GetByIdAsync(
                new UserId(command.UserId), cancellationToken);

            if (user == null)
            {
                logger.LogWarning("Username change failed: User {UserId} not found", command.UserId);
                return Result<UserDto>.Failure("User not found");
            }

            // Verifica se já existe usuário com o novo username
            logger.LogDebug("Checking username uniqueness for {NewUsername}", command.NewUsername);
            var existingUserWithUsername = await userRepository.GetByUsernameAsync(
                new Username(command.NewUsername), cancellationToken);

            if (existingUserWithUsername != null && existingUserWithUsername.Id != user.Id)
            {
                logger.LogWarning("Username change failed: Username {NewUsername} already in use by user {ExistingUserId}", 
                    command.NewUsername, existingUserWithUsername.Id);
                return Result<UserDto>.Failure("Username is already taken by another user");
            }

            var oldUsername = user.Username.Value;
            
            // Verificar rate limiting para mudanças de username
            if (!command.BypassRateLimit && !user.CanChangeUsername())
            {
                logger.LogWarning("Username change rate limit exceeded for user {UserId}. Last change: {LastChange}", 
                    command.UserId, user.LastUsernameChangeAt);
                return Result<UserDto>.Failure("Username can only be changed once per month");
            }

            // Aplica a alteração através do método de domínio
            logger.LogDebug("Applying username change from {OldUsername} to {NewUsername} for user {UserId}", 
                oldUsername, command.NewUsername, command.UserId);
            
            user.ChangeUsername(command.NewUsername);

            // Persiste as alterações
            var persistenceStart = stopwatch.ElapsedMilliseconds;
            await userRepository.UpdateAsync(user, cancellationToken);
            
            logger.LogDebug("Username change persistence completed in {ElapsedMs}ms", 
                stopwatch.ElapsedMilliseconds - persistenceStart);

            stopwatch.Stop();
            logger.LogInformation(
                "Username successfully changed for user {UserId} from {OldUsername} to {NewUsername} in {ElapsedMs}ms by {UpdatedBy}", 
                command.UserId, oldUsername, command.NewUsername, stopwatch.ElapsedMilliseconds, command.UpdatedBy ?? "System");

            return Result<UserDto>.Success(user.ToDto());
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, 
                "Unexpected error changing username for user {UserId} to {NewUsername} after {ElapsedMs}ms", 
                command.UserId, command.NewUsername, stopwatch.ElapsedMilliseconds);
            
            return Result<UserDto>.Failure($"Failed to change username: {ex.Message}");
        }
    }
}