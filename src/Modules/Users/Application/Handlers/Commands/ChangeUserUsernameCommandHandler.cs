using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Time;
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
/// <param name="dateTimeProvider">Provedor de data/hora para testabilidade</param>
/// <param name="logger">Logger estruturado para auditoria detalhada</param>
internal sealed class ChangeUserUsernameCommandHandler(
    IUserRepository userRepository,
    IDateTimeProvider dateTimeProvider,
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
            // Buscar e validar usuário
            var userResult = await GetAndValidateUserAsync(command, cancellationToken);
            if (userResult.IsFailure)
                return Result<UserDto>.Failure(userResult.Error);

            var user = userResult.Value;
            var oldUsername = user.Username.Value;

            // Validar rate limiting
            var rateLimitResult = ValidateRateLimit(command, user);
            if (rateLimitResult.IsFailure)
                return Result<UserDto>.Failure(rateLimitResult.Error);

            // Aplicar mudança de username
            ApplyUsernameChange(command, user, oldUsername);

            // Persistir alterações
            await PersistUsernameChangeAsync(user, stopwatch, cancellationToken);

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

    /// <summary>
    /// Busca o usuário e valida unicidade do novo username.
    /// </summary>
    private async Task<Result<Domain.Entities.User>> GetAndValidateUserAsync(
        ChangeUserUsernameCommand command,
        CancellationToken cancellationToken)
    {
        // Busca o usuário pelo ID
        logger.LogDebug("Fetching user {UserId} for username change", command.UserId);
        var user = await userRepository.GetByIdAsync(
            new UserId(command.UserId), cancellationToken);

        if (user == null)
        {
            logger.LogWarning("Username change failed: User {UserId} not found", command.UserId);
            return Result<Domain.Entities.User>.Failure("User not found");
        }

        // Verifica se já existe usuário com o novo username
        logger.LogDebug("Checking username uniqueness for {NewUsername}", command.NewUsername);
        var existingUserWithUsername = await userRepository.GetByUsernameAsync(
            new Username(command.NewUsername), cancellationToken);

        if (existingUserWithUsername != null && existingUserWithUsername.Id != user.Id)
        {
            logger.LogWarning("Username change failed: Username {NewUsername} already in use by user {ExistingUserId}",
                command.NewUsername, existingUserWithUsername.Id);
            return Result<Domain.Entities.User>.Failure("Username is already taken by another user");
        }

        return Result<Domain.Entities.User>.Success(user);
    }

    /// <summary>
    /// Valida regras de rate limiting para mudança de username.
    /// </summary>
    private Result<Unit> ValidateRateLimit(ChangeUserUsernameCommand command, Domain.Entities.User user)
    {
        if (!command.BypassRateLimit && !user.CanChangeUsername(dateTimeProvider))
        {
            logger.LogWarning("Username change rate limit exceeded for user {UserId}. Last change: {LastChange}",
                command.UserId, user.LastUsernameChangeAt);
            return Result<Unit>.Failure("Username can only be changed once per month");
        }

        return Result<Unit>.Success(Unit.Value);
    }

    /// <summary>
    /// Aplica a mudança de username usando o método de domínio.
    /// </summary>
    private void ApplyUsernameChange(ChangeUserUsernameCommand command, Domain.Entities.User user, string oldUsername)
    {
        logger.LogDebug("Applying username change from {OldUsername} to {NewUsername} for user {UserId}",
            oldUsername, command.NewUsername, command.UserId);

        user.ChangeUsername(command.NewUsername, dateTimeProvider);
    }

    /// <summary>
    /// Persiste as alterações de username no repositório.
    /// </summary>
    private async Task PersistUsernameChangeAsync(
        Domain.Entities.User user,
        System.Diagnostics.Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        var persistenceStart = stopwatch.ElapsedMilliseconds;
        await userRepository.UpdateAsync(user, cancellationToken);

        logger.LogDebug("Username change persistence completed in {ElapsedMs}ms",
            stopwatch.ElapsedMilliseconds - persistenceStart);
    }
}
