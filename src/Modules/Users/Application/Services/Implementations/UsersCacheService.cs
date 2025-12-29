using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Services.Interfaces;
using MeAjudaAi.Shared.Caching;

namespace MeAjudaAi.Modules.Users.Application.Services.Implementations;

/// <summary>
/// Serviço especializado de cache para o módulo Users.
/// Implementa estratégias específicas de cache e invalidação para entidades User.
/// </summary>
public class UsersCacheService(ICacheService cacheService) : IUsersCacheService
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan LongExpiration = TimeSpan.FromHours(2);

    /// <summary>
    /// Obtém ou cria cache para usuário por ID
    /// IMPORTANT: Does NOT cache NULL values to avoid caching "not found" results
    /// </summary>
    public async Task<UserDto?> GetOrCacheUserByIdAsync(
        Guid userId,
        Func<CancellationToken, ValueTask<UserDto?>> factory,
        CancellationToken cancellationToken = default)
    {
        var key = UsersCacheKeys.UserById(userId);
        var tags = CacheTags.GetUserRelatedTags(userId);

        // Try to get from cache first
        var (cachedValue, isCached) = await cacheService.GetAsync<UserDto>(key, cancellationToken);
        if (isCached && cachedValue != null)
        {
            return cachedValue;
        }

        // Cache miss - call factory
        var result = await factory(cancellationToken);
        
        // Only cache non-null results (avoid caching "not found")
        if (result != null)
        {
            await cacheService.SetAsync(key, result, DefaultExpiration, tags: tags, cancellationToken: cancellationToken);
        }

        return result;
    }

    /// <summary>
    /// Define o cache para um usuário específico
    /// </summary>
    public async Task SetUserAsync(UserDto user, CancellationToken cancellationToken = default)
    {
        var key = UsersCacheKeys.UserById(user.Id);
        var tags = CacheTags.GetUserRelatedTags(user.Id, user.Email);

        await cacheService.SetAsync(
            key,
            user,
            DefaultExpiration,
            tags: tags,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Obtém ou cria cache para configurações do sistema de usuários
    /// </summary>
    public async Task<T> GetOrCacheSystemConfigAsync<T>(
        Func<CancellationToken, ValueTask<T>> factory,
        CancellationToken cancellationToken = default)
    {
        var key = UsersCacheKeys.UserSystemConfig;
        var tags = new[] { CacheTags.Configuration, CacheTags.Users };

        return await cacheService.GetOrCreateAsync(
            key,
            factory,
            LongExpiration, // Configurações mudam raramente
            tags: tags,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Invalida todo o cache relacionado a um usuário específico
    /// </summary>
    public async Task InvalidateUserAsync(Guid userId, string? email = null, CancellationToken cancellationToken = default)
    {
        // Remove cache específico do usuário
        await cacheService.RemoveAsync(UsersCacheKeys.UserById(userId), cancellationToken);

        if (!string.IsNullOrEmpty(email))
        {
            await cacheService.RemoveAsync(UsersCacheKeys.UserByEmail(email), cancellationToken);
        }

        // Remove cache dos roles do usuário
        await cacheService.RemoveAsync(UsersCacheKeys.UserRoles(userId), cancellationToken);

        // Invalida listas que podem conter este usuário
        await cacheService.RemoveByPatternAsync(CacheTags.UsersList, cancellationToken);
    }
}
