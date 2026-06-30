using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules;
using MeAjudaAi.Contracts.Modules.Users;
using MeAjudaAi.Contracts.Modules.Users.DTOs;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Mappers;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.Application.Queries.Interfaces;
using MeAjudaAi.Shared.Queries;

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
    IUserQueries userQueries) : IUsersModuleApi
{
    private static class ModuleMetadata
    {
        public const string Name = "Users";
        public const string Version = "1.0";
    }

    public string ModuleName => ModuleMetadata.Name;
    public string ApiVersion => ModuleMetadata.Version;

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await userQueries.CanConnectAsync(cancellationToken);
    }

    public async Task<Result<ModuleUserDto?>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = new GetUserByIdQuery(userId);
        var result = await getUserByIdHandler.HandleAsync(query, cancellationToken);

        return result.Match(
            user => user is null
                ? Result<ModuleUserDto?>.Success(null)
                : Result<ModuleUserDto?>.Success(user.ToContract()),
            error => error.StatusCode == 404
                ? Result<ModuleUserDto?>.Success(null)
                : Result<ModuleUserDto?>.Failure(error));
    }

    public async Task<Result<ModuleUserDto?>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var query = new GetUserByEmailQuery(email);
        var result = await getUserByEmailHandler.HandleAsync(query, cancellationToken);

        return result.Match(
            user => user is null
                ? Result<ModuleUserDto?>.Success(null)
                : Result<ModuleUserDto?>.Success(user.ToContract()),
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
                userDtos.ToBasicContract()),
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
