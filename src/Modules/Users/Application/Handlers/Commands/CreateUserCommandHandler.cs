using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Application.Services.Interfaces;
using MeAjudaAi.Modules.Users.Domain.Repositories;
using MeAjudaAi.Modules.Users.Domain.Services;
using MeAjudaAi.Modules.Users.Domain.ValueObjects;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Functional;
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

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            logger.LogInformation("Starting user creation process for {Email}", command.Email);

            // Verificar duplicidade de email e username
            var uniquenessResult = await ValidateUniquenessAsync(command, cancellationToken);
            if (uniquenessResult.IsFailure)
                return Result<UserDto>.Failure(uniquenessResult.Error);

            // Criar usuário através do serviço de domínio
            var userResult = await CreateUserAsync(command, stopwatch, cancellationToken);
            if (userResult.IsFailure)
                return Result<UserDto>.Failure(userResult.Error);

            // Persistir usuário no repositório
            await PersistUserAsync(userResult.Value, stopwatch, cancellationToken);

            stopwatch.Stop();
            logger.LogInformation("User {UserId} created successfully for email {Email} in {ElapsedMs}ms",
                userResult.Value.Id, command.Email, stopwatch.ElapsedMilliseconds);

            return Result<UserDto>.Success(userResult.Value.ToDto());
        }
        catch (ArgumentException)
        {
            // Permite que ArgumentException (erros de validação) propague para GlobalExceptionHandler
            throw;
        }
        catch (MeAjudaAi.Shared.Exceptions.ValidationException)
        {
            // Permite que ValidationException propague para GlobalExceptionHandler
            throw;
        }
        catch (MeAjudaAi.Shared.Exceptions.DomainException)
        {
            // Permite que DomainException (violações de regras de negócio) propague para GlobalExceptionHandler
            throw;
        }
        catch (Exception ex)
        {
            // Capturar erros de infraestrutura (database, cache, etc.) e logar com detalhes completos
            logger.LogError(ex, "Unexpected error creating user with email {Email}. ExceptionType: {ExceptionType}, Message: {Message}", 
                command.Email, ex.GetType().Name, ex.Message);
            return Result<UserDto>.Failure("Falha ao criar usuário. Tente novamente mais tarde.");
        }
    }

    /// <summary>
    /// Valida se o email e username são únicos no sistema.
    /// </summary>
    private async Task<Result<Unit>> ValidateUniquenessAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        // Verifica se já existe usuário com o email informado
        logger.LogDebug("Checking email uniqueness for {Email}", command.Email);
        var existingByEmail = await userRepository.GetByEmailAsync(
            new Email(command.Email), cancellationToken);
        if (existingByEmail != null)
        {
            logger.LogWarning("User creation failed: Email {Email} already exists", command.Email);
            return Result<Unit>.Failure("Usuário com este email já existe");
        }

        // Verifica se já existe usuário com o username informado
        logger.LogDebug("Checking username uniqueness for {Username}", command.Username);
        var existingByUsername = await userRepository.GetByUsernameAsync(
            new Username(command.Username), cancellationToken);
        if (existingByUsername != null)
        {
            logger.LogWarning("User creation failed: Username {Username} already exists", command.Username);
            return Result<Unit>.Failure("Nome de usuário já está sendo utilizado");
        }

        return Result<Unit>.Success(Unit.Value);
    }

    /// <summary>
    /// Cria o usuário através do serviço de domínio.
    /// </summary>
    private async Task<Result<Domain.Entities.User>> CreateUserAsync(
        CreateUserCommand command,
        System.Diagnostics.Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Creating user domain entity for email {Email}, username {Username}",
            command.Email, command.Username);

        var userCreationStart = stopwatch.ElapsedMilliseconds;
        var userResult = await userDomainService.CreateUserAsync(
            new Username(command.Username),
            new Email(command.Email),
            command.FirstName,
            command.LastName,
            command.Password,
            command.Roles,
            command.PhoneNumber,
            cancellationToken);

        logger.LogDebug("User domain service completed in {ElapsedMs}ms",
            stopwatch.ElapsedMilliseconds - userCreationStart);

        if (userResult.IsFailure)
        {
            logger.LogError("User creation failed for email {Email}: {Error}", command.Email, userResult.Error);
        }

        return userResult;
    }

    /// <summary>
    /// Persiste o usuário no repositório.
    /// </summary>
    private async Task PersistUserAsync(
        Domain.Entities.User user,
        System.Diagnostics.Stopwatch stopwatch,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Persisting user {UserId} to repository", user.Id);
        var persistenceStart = stopwatch.ElapsedMilliseconds;
        await userRepository.AddAsync(user, cancellationToken);

        logger.LogDebug("User persistence completed in {ElapsedMs}ms",
            stopwatch.ElapsedMilliseconds - persistenceStart);
    }
}
