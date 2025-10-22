using MeAjudaAi.Modules.Users.Application.DTOs;

namespace MeAjudaAi.Modules.Users.Application.Caching;

/// <summary>
/// Interface para serviço especializado de cache do módulo Users.
/// </summary>
public interface IUsersCacheService
{
    /// <summary>
    /// Obtém ou cria cache para usuário por ID
    /// </summary>
    Task<UserDto?> GetOrCacheUserByIdAsync(
        Guid userId,
        Func<CancellationToken, ValueTask<UserDto?>> factory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém ou cria cache para configurações do sistema de usuários
    /// </summary>
    Task<T> GetOrCacheSystemConfigAsync<T>(
        Func<CancellationToken, ValueTask<T>> factory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalida todo o cache relacionado a um usuário específico
    /// </summary>
    Task InvalidateUserAsync(Guid userId, string? email = null, CancellationToken cancellationToken = default);
}
