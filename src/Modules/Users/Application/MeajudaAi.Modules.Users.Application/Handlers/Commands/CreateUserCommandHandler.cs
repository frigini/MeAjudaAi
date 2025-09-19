using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Common;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Application.Handlers.Commands;

/// <summary>
/// Handler responsável por processar comandos de criação de usuários.
/// </summary>
/// <remarks>
/// Implementa o padrão CQRS para criação de usuários, incluindo validações de negócio,
/// verificação de duplicidade de email/username e integração com serviços de domínio.
/// Utiliza o IUserDomainService para encapsular a lógica de criação de usuários com
/// integração ao Keycloak.
/// </remarks>
/// <param name="userDomainService">Serviço de domínio para operações de usuário</param>
/// <param name="userRepository">Repositório para persistência de usuários</param>
/// <param name="logger">Logger estruturado para auditoria e debugging</param>
internal sealed class CreateUserCommandHandler(
    IUserDomainService userDomainService,
    IUserRepository userRepository,
    ILogger<CreateUserCommandHandler> logger
) : ICommandHandler<CreateUserCommand, Result<UserDto>>
{
    /// <summary>
    /// Processa o comando de criação de usuário de forma assíncrona.
    /// </summary>
    /// <param name="command">Comando contendo os dados do usuário a ser criado</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado da operação contendo:
    /// - Sucesso: UserDto com os dados do usuário criado
    /// - Falha: Mensagem de erro descritiva
    /// </returns>
    /// <remarks>
    /// O processo inclui:
    /// 1. Verificação de duplicidade de email e username
    /// 2. Criação do usuário através do serviço de domínio
    /// 3. Persistência no repositório
    /// 4. Retorno do DTO do usuário criado
    /// 
    /// Todas as exceções são capturadas e convertidas em resultados de falha.
    /// </remarks>
    public async Task<Result<UserDto>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        using var activity = logger.BeginScope(new Dictionary<string, object>
        {
            ["UserId"] = command.CorrelationId,
            ["Email"] = command.Email,
            ["Username"] = command.Username,
            ["Operation"] = "CreateUser"
        });

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        logger.LogInformation("Starting user creation process for {Email}", command.Email);

        try
        {
            // Verifica se já existe usuário com o email informado
            logger.LogDebug("Checking email uniqueness for {Email}", command.Email);
            var existingByEmail = await userRepository.GetByEmailAsync(
                new Email(command.Email), cancellationToken);
            if (existingByEmail != null)
            {
                logger.LogWarning("User creation failed: Email {Email} already exists", command.Email);
                return Result<UserDto>.Failure("User with this email already exists");
            }

            // Verifica se já existe usuário com o username informado
            logger.LogDebug("Checking username uniqueness for {Username}", command.Username);
            var existingByUsername = await userRepository.GetByUsernameAsync(
                new Username(command.Username), cancellationToken);
            if (existingByUsername != null)
            {
                logger.LogWarning("User creation failed: Username {Username} already exists", command.Username);
                return Result<UserDto>.Failure("Username already taken");
            }

            logger.LogDebug("Creating user domain entity for email {Email}, username {Username}", 
                command.Email, command.Username);

            // Cria o usuário através do serviço de domínio
            var userCreationStart = stopwatch.ElapsedMilliseconds;
            var userResult = await userDomainService.CreateUserAsync(
                new Username(command.Username),
                new Email(command.Email),
                command.FirstName,
                command.LastName,
                command.Password,
                command.Roles,
                cancellationToken);

            logger.LogDebug("User domain service completed in {ElapsedMs}ms", 
                stopwatch.ElapsedMilliseconds - userCreationStart);

            if (userResult.IsFailure)
            {
                logger.LogError("User creation failed for email {Email}: {Error}", command.Email, userResult.Error);
                return Result<UserDto>.Failure(userResult.Error);
            }

            var user = userResult.Value;

            // Persiste o usuário no repositório
            logger.LogDebug("Persisting user {UserId} to repository", user.Id);
            var persistenceStart = stopwatch.ElapsedMilliseconds;
            await userRepository.AddAsync(user, cancellationToken);
            
            logger.LogDebug("User persistence completed in {ElapsedMs}ms", 
                stopwatch.ElapsedMilliseconds - persistenceStart);

            stopwatch.Stop();
            logger.LogInformation("User {UserId} created successfully for email {Email} in {ElapsedMs}ms", 
                user.Id, command.Email, stopwatch.ElapsedMilliseconds);

            return Result<UserDto>.Success(user.ToDto());
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Unexpected error creating user for email {Email} after {ElapsedMs}ms", 
                command.Email, stopwatch.ElapsedMilliseconds);
            return Result<UserDto>.Failure($"Failed to create user: {ex.Message}");
        }
    }
}