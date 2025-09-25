using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Shared.Contracts.Modules;
using MeAjudaAi.Shared.Contracts.Modules.Users;
using MeAjudaAi.Shared.Contracts.Modules.Users.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Users.Application.Services;

/// <summary>
/// Implementação da API pública do módulo Users para outros módulos
/// </summary>
[ModuleApi("Users", "1.0")]
public sealed class UsersModuleApi(
    IQueryHandler<GetUserByIdQuery, Result<UserDto>> getUserByIdHandler,
    IQueryHandler<GetUserByEmailQuery, Result<UserDto>> getUserByEmailHandler) : IUsersModuleApi, IModuleApi
{
    public string ModuleName => "Users";
    public string ApiVersion => "1.0";

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        // Verifica se o módulo Users está funcionando
        return Task.FromResult(true); // Por enquanto sempre true, pode incluir health checks
    }

    public async Task<Result<ModuleUserDto?>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = new GetUserByIdQuery(userId);
        var result = await getUserByIdHandler.HandleAsync(query, cancellationToken);
        
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
            onFailure: error => Result<ModuleUserDto?>.Failure(error)
        );
    }

    public async Task<Result<ModuleUserDto?>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var query = new GetUserByEmailQuery(email);
        var result = await getUserByEmailHandler.HandleAsync(query, cancellationToken);
        
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
            onFailure: error => Result<ModuleUserDto?>.Failure(error)
        );
    }

    public async Task<Result<IReadOnlyList<ModuleUserBasicDto>>> GetUsersBatchAsync(
        IReadOnlyList<Guid> userIds, 
        CancellationToken cancellationToken = default)
    {
        var users = new List<ModuleUserBasicDto>();
        
        // Para cada ID, busca o usuário (otimização futura: query batch)
        foreach (var userId in userIds)
        {
            var userResult = await GetUserByIdAsync(userId, cancellationToken);
            if (userResult.IsSuccess && userResult.Value != null)
            {
                var user = userResult.Value;
                users.Add(new ModuleUserBasicDto(user.Id, user.Username, user.Email, true));
            }
        }
        
        return Result<IReadOnlyList<ModuleUserBasicDto>>.Success(users);
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
        // TODO: Implementar quando houver GetUserByUsernameQuery
        // Por enquanto, retorna false (não implementado)
        await Task.CompletedTask;
        return Result<bool>.Success(false);
    }
}