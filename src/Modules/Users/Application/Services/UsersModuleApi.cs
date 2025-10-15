using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Shared.Contracts.Modules;
using MeAjudaAi.Shared.Contracts.Modules.Users;
using MeAjudaAi.Shared.Contracts.Modules.Users.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Users.Application.Services;

/// <summary>
/// Implementação da API pública do módulo Users para outros módulos
/// </summary>
[ModuleApi("Users", "1.0")]
public sealed class UsersModuleApi : IUsersModuleApi, IModuleApi
{
    private readonly IQueryHandler<GetUserByIdQuery, Result<UserDto>> _getUserByIdHandler;
    private readonly IQueryHandler<GetUserByEmailQuery, Result<UserDto>> _getUserByEmailHandler;
    private readonly IQueryHandler<GetUserByUsernameQuery, Result<UserDto>> _getUserByUsernameHandler;
    private readonly IQueryHandler<GetUsersByIdsQuery, Result<IReadOnlyList<UserDto>>> _getUsersByIdsHandler;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UsersModuleApi> _logger;

    public UsersModuleApi(
        IQueryHandler<GetUserByIdQuery, Result<UserDto>> getUserByIdHandler,
        IQueryHandler<GetUserByEmailQuery, Result<UserDto>> getUserByEmailHandler,
        IQueryHandler<GetUserByUsernameQuery, Result<UserDto>> getUserByUsernameHandler,
        IQueryHandler<GetUsersByIdsQuery, Result<IReadOnlyList<UserDto>>> getUsersByIdsHandler,
        IServiceProvider serviceProvider,
        ILogger<UsersModuleApi> logger)
    {
        _getUserByIdHandler = getUserByIdHandler;
        _getUserByEmailHandler = getUserByEmailHandler;
        _getUserByUsernameHandler = getUserByUsernameHandler;
        _getUsersByIdsHandler = getUsersByIdsHandler;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    public string ModuleName => "Users";
    public string ApiVersion => "1.0";

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking Users module availability");
            
            // Verifica health checks registrados do sistema
            var healthCheckService = _serviceProvider.GetService<HealthCheckService>();
            if (healthCheckService != null)
            {
                var healthReport = await healthCheckService.CheckHealthAsync(
                    check => check.Tags.Contains("users") || check.Tags.Contains("database"), 
                    cancellationToken);
                
                // Se algum health check crítico falhou, o módulo não está disponível
                if (healthReport.Status == HealthStatus.Unhealthy)
                {
                    _logger.LogWarning("Users module unavailable due to failed health checks: {FailedChecks}", 
                        string.Join(", ", healthReport.Entries.Where(e => e.Value.Status == HealthStatus.Unhealthy).Select(e => e.Key)));
                    return false;
                }
            }

            // Testa funcionalidade básica - verifica se os handlers essenciais estão disponíveis
            var canExecuteBasicOperations = await CanExecuteBasicOperationsAsync(cancellationToken);
            if (!canExecuteBasicOperations)
            {
                _logger.LogWarning("Users module unavailable - basic operations test failed");
                return false;
            }

            _logger.LogDebug("Users module is available and healthy");
            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Users module availability check was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Users module availability");
            return false;
        }
    }

    /// <summary>
    /// Testa se as operações básicas do módulo estão funcionando
    /// </summary>
    private async Task<bool> CanExecuteBasicOperationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Testa uma operação simples que não deveria falhar (mesmo que usuário não exista)
            // Usamos um GUID fixo que provavelmente não existe, mas o handler deve responder adequadamente
            var testQuery = new GetUserByIdQuery(Guid.Parse("00000000-0000-0000-0000-000000000001"));
            var result = await _getUserByIdHandler.HandleAsync(testQuery, cancellationToken);
            
            // Se chegou até aqui sem exception, os handlers estão funcionais
            // Não importa se o usuário existe ou não, importa que o sistema respondeu
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Basic operations test failed for Users module");
            return false;
        }
    }

    public async Task<Result<ModuleUserDto?>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = new GetUserByIdQuery(userId);
        var result = await _getUserByIdHandler.HandleAsync(query, cancellationToken);

        return result.Match(
            onSuccess: userDto => userDto == null
                ? Result<ModuleUserDto?>.Success(null)
                : Result<ModuleUserDto?>.Success(new ModuleUserDto(
                    userDto.Id,
                    userDto.Username,
                    userDto.Email,
                    userDto.FirstName,
                    userDto.LastName,
                    userDto.FullName)),
            onFailure: error => error.StatusCode == 404
                ? Result<ModuleUserDto?>.Success(null)  // NotFound -> Success(null)
                : Result<ModuleUserDto?>.Failure(error) // Outros erros propagam
        );
    }

    public async Task<Result<ModuleUserDto?>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var query = new GetUserByEmailQuery(email);
        var result = await _getUserByEmailHandler.HandleAsync(query, cancellationToken);

        return result.Match(
            onSuccess: userDto => userDto == null
                ? Result<ModuleUserDto?>.Success(null)
                : Result<ModuleUserDto?>.Success(new ModuleUserDto(
                    userDto.Id,
                    userDto.Username,
                    userDto.Email,
                    userDto.FirstName,
                    userDto.LastName,
                    userDto.FullName)),
            onFailure: error => error.StatusCode == 404
                ? Result<ModuleUserDto?>.Success(null)  // NotFound -> Success(null)
                : Result<ModuleUserDto?>.Failure(error) // Outros erros propagam
        );
    }

    public async Task<Result<IReadOnlyList<ModuleUserBasicDto>>> GetUsersBatchAsync(
        IReadOnlyList<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        // Usar query batch em vez de N queries individuais
        var batchQuery = new GetUsersByIdsQuery(userIds);
        var result = await _getUsersByIdsHandler.HandleAsync(batchQuery, cancellationToken);

        return result.Match(
            onSuccess: userDtos => Result<IReadOnlyList<ModuleUserBasicDto>>.Success(
                userDtos.Select(user => new ModuleUserBasicDto(user.Id, user.Username, user.Email, true)).ToList()),
            onFailure: error => Result<IReadOnlyList<ModuleUserBasicDto>>.Failure(error)
        );
    }

    public async Task<Result<bool>> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await GetUserByIdAsync(userId, cancellationToken);
        return result.Match(
            onSuccess: user => Result<bool>.Success(user != null),
            onFailure: _ => Result<bool>.Success(false) // Em caso de erro, assume que não existe
        );
    }

    public async Task<Result<bool>> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var result = await GetUserByEmailAsync(email, cancellationToken);
        return result.Match(
            onSuccess: user => Result<bool>.Success(user != null),
            onFailure: _ => Result<bool>.Success(false) // Em caso de erro, assume que não existe
        );
    }

    public async Task<Result<bool>> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        var query = new GetUserByUsernameQuery(username);
        var result = await _getUserByUsernameHandler.HandleAsync(query, cancellationToken);

        return result.IsSuccess
            ? Result<bool>.Success(true)  // Usuário encontrado = username existe
            : Result<bool>.Success(false); // Usuário não encontrado = username não existe
    }
}