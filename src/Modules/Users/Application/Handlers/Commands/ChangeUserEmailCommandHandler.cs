using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de alteração de email de usuários.
/// </summary>
/// <remarks>
/// **Operação Crítica de Segurança:**
/// Este handler processa alterações de email que são operações sensíveis
/// envolvendo validações rigorosas e possível sincronização externa.
/// 
/// **Responsabilidades:**
/// - Validação de existência do usuário
/// - Verificação de unicidade do novo email
/// - Aplicação de regras de negócio específicas
/// - Logging detalhado para auditoria de segurança
/// - Persistência das alterações
/// 
/// **Integrações:**
/// - Keycloak (sincronização futura)
/// - Sistema de notificações por email
/// - Logs de auditoria de segurança
/// </remarks>
/// <param name="userRepository">Repositório para operações de usuário</param>
/// <param name="logger">Logger estruturado para auditoria detalhada</param>
public sealed class ChangeUserEmailCommandHandler(
    IUserRepository userRepository,
    ILogger<ChangeUserEmailCommandHandler> logger
) : ICommandHandler<ChangeUserEmailCommand, Result<UserDto>>
{
    /// <summary>
    /// Processa o comando de alteração de email de forma assíncrona.
    /// </summary>
    /// <param name="command">Comando contendo ID do usuário e novo email</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado da operação contendo:
    /// - Sucesso: UserDto com email atualizado
    /// - Falha: Mensagem descritiva do erro
    /// </returns>
    /// <remarks>
    /// **Fluxo de Segurança:**
    /// 1. ✅ Validação de existência do usuário
    /// 2. ✅ Verificação de unicidade do email
    /// 3. ✅ Aplicação de regras de domínio (User.ChangeEmail)
    /// 4. ✅ Logging detalhado para auditoria
    /// 5. ✅ Persistência atomica das alterações
    /// 
    /// **Validações Automáticas:**
    /// - Formato válido de email
    /// - Tamanho dentro dos limites
    /// - Usuário não deletado
    /// - Email único no sistema
    /// </remarks>
    public async Task<Result<UserDto>> HandleAsync(
        ChangeUserEmailCommand command,
        CancellationToken cancellationToken = default)
    {
        using var activity = logger.BeginScope(new Dictionary<string, object>
        {
            ["UserId"] = command.UserId,
            ["NewEmail"] = command.NewEmail,
            ["UpdatedBy"] = command.UpdatedBy ?? "Unknown",
            ["Operation"] = "ChangeEmail"
        });

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        logger.LogInformation("Starting email change process for user {UserId} to {NewEmail}",
            command.UserId, command.NewEmail);

        try
        {
            // Buscar e validar usuário
            var userResult = await GetAndValidateUserAsync(command, cancellationToken);
            if (userResult.IsFailure)
                return Result<UserDto>.Failure(userResult.Error);

            var user = userResult.Value;
            var oldEmail = user.Email.Value;

            // Aplicar mudança de email
            ApplyEmailChange(command, user, oldEmail);

            // Persistir alterações
            await PersistEmailChangeAsync(user, stopwatch, cancellationToken);

            stopwatch.Stop();
            logger.LogInformation(
                "Email successfully changed for user {UserId} from {OldEmail} to {NewEmail} in {ElapsedMs}ms by {UpdatedBy}",
                command.UserId, oldEmail, command.NewEmail, stopwatch.ElapsedMilliseconds, command.UpdatedBy ?? "System");

            return Result<UserDto>.Success(user.ToDto());
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
            stopwatch.Stop();
            logger.LogError(ex,
                "Unexpected error changing email for user {UserId} to {NewEmail} after {ElapsedMs}ms",
                command.UserId, command.NewEmail, stopwatch.ElapsedMilliseconds);

            return Result<UserDto>.Failure($"Failed to change user email: {ex.Message}");
        }
    }

    /// <summary>
    /// Busca o usuário e valida unicidade do novo email.
    /// </summary>
    private async Task<Result<Domain.Entities.User>> GetAndValidateUserAsync(
        ChangeUserEmailCommand command,
        CancellationToken cancellationToken)
    {
        // Busca o usuário pelo ID
        logger.LogDebug("Fetching user {UserId} for email change", command.UserId);
        var user = await userRepository.GetByIdAsync(
            new UserId(command.UserId), cancellationToken);

        if (user == null)
        {
            logger.LogWarning("Email change failed: User {UserId} not found", command.UserId);
            return Result<Domain.Entities.User>.Failure("User not found");
        }

        // Verifica se já existe usuário com o novo email
        logger.LogDebug("Checking email uniqueness for {NewEmail}", command.NewEmail);
        var existingUserWithEmail = await userRepository.GetByEmailAsync(
            new Email(command.NewEmail), cancellationToken);

        if (existingUserWithEmail != null && existingUserWithEmail.Id != user.Id)
        {
            logger.LogWarning("Email change failed: Email {NewEmail} already in use by user {ExistingUserId}",
                command.NewEmail, existingUserWithEmail.Id);
            return Result<Domain.Entities.User>.Failure("Email address is already in use by another user");
        }

        return Result<Domain.Entities.User>.Success(user);
    }

    /// <summary>
    /// Aplica a mudança de email usando o método de domínio.
    /// </summary>
    private void ApplyEmailChange(ChangeUserEmailCommand command, Domain.Entities.User user, string oldEmail)
    {
        logger.LogDebug("Applying email change from {OldEmail} to {NewEmail} for user {UserId}",
            oldEmail, command.NewEmail, command.UserId);

        user.ChangeEmail(command.NewEmail);
    }

    /// <summary>
    /// Persiste as alterações de email no repositório.
    /// </summary>
    private async Task PersistEmailChangeAsync(
        Domain.Entities.User user,
        System.Diagnostics.Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        var persistenceStart = stopwatch.ElapsedMilliseconds;
        await userRepository.UpdateAsync(user, cancellationToken);

        logger.LogDebug("Email change persistence completed in {ElapsedMs}ms",
            stopwatch.ElapsedMilliseconds - persistenceStart);
    }
}
