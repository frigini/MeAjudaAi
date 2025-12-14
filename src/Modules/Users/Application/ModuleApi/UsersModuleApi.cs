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

namespace MeAjudaAi.Modules.Users.Application.ModuleApi;

/// <summary>
/// Implementação da API pública do módulo Users para outros módulos
/// </summary>
[ModuleApi(ModuleMetadata.Name, ModuleMetadata.Version)]
public sealed class UsersModuleApi(
    IQueryHandler<GetUserByIdQuery, Result<UserDto>> getUserByIdHandler,
    IQueryHandler<GetUserByEmailQuery, Result<UserDto>> getUserByEmailHandler,
    IQueryHandler<GetUserByUsernameQuery, Result<UserDto>> getUserByUsernameHandler,
    IQueryHandler<GetUsersByIdsQuery, Result<IReadOnlyList<UserDto>>> getUsersByIdsHandler,
    IServiceProvider serviceProvider,
    ILogger<UsersModuleApi> logger) : IUsersModuleApi, IModuleApi
{
    private static class ModuleMetadata
    {
        public const string Name = "Users";
        public const string Version = "1.0";
    }

    public string ModuleName => ModuleMetadata.Name;
    public string ApiVersion => ModuleMetadata.Version;

    private static readonly Guid HealthCheckUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private static ModuleUserDto MapToModuleUserDto(UserDto user) =>
        new(user.Id, user.Username, user.Email, user.FirstName, user.LastName, user.FullName);

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogDebug("Checking Users module availability");

            // Verifica health checks registrados do sistema
            var healthCheckService = serviceProvider.GetService<HealthCheckService>();
            if (healthCheckService != null)
            {
                var healthReport = await healthCheckService.CheckHealthAsync(
                    check => check.Tags.Contains("users") || check.Tags.Contains("database"),
                    cancellationToken);

                // Se algum health check crítico falhou, o módulo não está disponível
                if (healthReport.Status == HealthStatus.Unhealthy)
                {
                    logger.LogWarning("Users module unavailable due to failed health checks: {FailedChecks}",
                        string.Join(", ", healthReport.Entries.Where(e => e.Value.Status == HealthStatus.Unhealthy).Select(e => e.Key)));
                    return false;
                }
            }

            // Testa funcionalidade básica - verifica se os handlers essenciais estão disponíveis
            var canExecuteBasicOperations = await CanExecuteBasicOperationsAsync(cancellationToken);
            if (!canExecuteBasicOperations)
            {
                logger.LogWarning("Users module unavailable - basic operations test failed");
                return false;
            }

            logger.LogDebug("Users module is available and healthy");
            return true;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogDebug(ex, "Users module availability check was cancelled");
            throw new InvalidOperationException("Users module availability check was cancelled", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking Users module availability");
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
            // Tenta buscar um usuário fictício (espera-se NotFound, mas valida que a infraestrutura está OK)
            var testQuery = new GetUserByIdQuery(HealthCheckUserId);
            var result = await getUserByIdHandler.HandleAsync(testQuery, cancellationToken);

            // Verifica o resultado da operação para detectar falhas de infraestrutura
            if (result.IsSuccess)
            {
                // Operação bem-sucedida, sistema está saudável
                return true;
            }

            // Se falhou, verifica se é um erro aceitável (NotFound) ou uma falha real
            if (result.Error.StatusCode == 404)
            {
                // NotFound é aceitável para o health check - significa que o sistema respondeu corretamente
                return true;
            }

            // Qualquer outro erro (500, timeout de DB, etc.) indica problema de infraestrutura
            logger.LogWarning("Basic operations test failed with non-404 error: {ErrorMessage} (Status: {StatusCode})",
                result.Error.Message, result.Error.StatusCode);
            return false;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Basic operations test failed for Users module");
            return false;
        }
    }

    public async Task<Result<ModuleUserDto?>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = new GetUserByIdQuery(userId);
        var result = await getUserByIdHandler.HandleAsync(query, cancellationToken);

        return result.Match<Result<ModuleUserDto?>>(
            user => user is null
                ? Result<ModuleUserDto?>.Success(null)
                : Result<ModuleUserDto?>.Success(MapToModuleUserDto(user)),
            error => error.StatusCode == 404
                ? Result<ModuleUserDto?>.Success(null)
                : Result<ModuleUserDto?>.Failure(error));
    }

    public async Task<Result<ModuleUserDto?>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var query = new GetUserByEmailQuery(email);
        var result = await getUserByEmailHandler.HandleAsync(query, cancellationToken);

        return result.Match<Result<ModuleUserDto?>>(
            user => user is null
                ? Result<ModuleUserDto?>.Success(null)
                : Result<ModuleUserDto?>.Success(MapToModuleUserDto(user)),
            error => error.StatusCode == 404
                ? Result<ModuleUserDto?>.Success(null)
                : Result<ModuleUserDto?>.Failure(error));
    }

    public async Task<Result<IReadOnlyList<ModuleUserBasicDto>>> GetUsersBatchAsync(
        IReadOnlyList<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        // Usar query batch em vez de N queries individuais
        var batchQuery = new GetUsersByIdsQuery(userIds);
        var result = await getUsersByIdsHandler.HandleAsync(batchQuery, cancellationToken);

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
        var result = await getUserByUsernameHandler.HandleAsync(query, cancellationToken);

        return result.Match(
            onSuccess: _ => Result<bool>.Success(true),
            onFailure: _ => Result<bool>.Success(false) // Any error means user doesn't exist
        );
    }
}
