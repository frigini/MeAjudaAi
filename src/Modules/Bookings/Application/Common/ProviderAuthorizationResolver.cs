using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MeAjudaAi.Modules.Bookings.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace MeAjudaAi.Modules.Bookings.Application.Common;

public enum AuthorizationFailureKind
{
    None,
    Unauthorized,
    UpstreamFailure,
    NotLinked
}

[ExcludeFromCodeCoverage]
public sealed class ProviderAuthorizationResult
{
    public Guid? UserId { get; init; }
    public bool IsAdmin { get; init; }
    public Guid? ProviderId { get; init; }
    public AuthorizationFailureKind FailureKind { get; init; }
    public string? ErrorMessage { get; init; }
    public int? ErrorStatusCode { get; init; }

    public static ProviderAuthorizationResult Admin(Guid userId) => new() { IsAdmin = true, UserId = userId };
    public static ProviderAuthorizationResult Authorized(Guid userId, Guid providerId) => new() { UserId = userId, ProviderId = providerId };
    public static ProviderAuthorizationResult NotLinked(Guid userId) => new() { UserId = userId, FailureKind = AuthorizationFailureKind.NotLinked };
    public static ProviderAuthorizationResult Unauthorized(string? message = null) => 
        new() { FailureKind = AuthorizationFailureKind.Unauthorized, ErrorMessage = message };
    public static ProviderAuthorizationResult UpstreamFailure(string message, int statusCode) => 
        new() { FailureKind = AuthorizationFailureKind.UpstreamFailure, ErrorMessage = message, ErrorStatusCode = statusCode };
}

public sealed class ProviderAuthorizationResolver(
    ICacheService cache,
    IProvidersModuleApi providersApi,
    ILogger<ProviderAuthorizationResolver> logger)
{
    private const string CacheKeyPrefix = "bookings:provider_by_user:";
    private static readonly TimeSpan LocalCacheExpiration = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan AbsoluteExpiration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Invalida o cache do usuário especificado.
    /// </summary>
    public async Task InvalidateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{userId}";
        await cache.RemoveAsync(cacheKey, cancellationToken);
        logger.LogInformation("Cache invalidated for user {UserId}", userId);
    }

    /// <summary>
    /// Resolve o ProviderId vinculado ao usuário autenticado.
    /// </summary>
    public async Task<ProviderAuthorizationResult> ResolveAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        var userIdClaim = user.FindFirst(AuthConstants.Claims.Subject)?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var uId))
        {
            return ProviderAuthorizationResult.Unauthorized("Identificação do usuário não encontrada ou inválida.");
        }

        var isSystemAdmin = string.Equals(user.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, "true", StringComparison.OrdinalIgnoreCase);

        if (isSystemAdmin)
        {
            return ProviderAuthorizationResult.Admin(uId);
        }

        var providerIdClaim = user.FindFirst(AuthConstants.Claims.ProviderId)?.Value;
        if (!string.IsNullOrEmpty(providerIdClaim) && Guid.TryParse(providerIdClaim, out var pId) && pId != Guid.Empty)
        {
            return ProviderAuthorizationResult.Authorized(uId, pId);
        }

        var cacheKey = $"{CacheKeyPrefix}{uId}";

        try
        {
            var options = new HybridCacheEntryOptions
            {
                Expiration = AbsoluteExpiration,
                LocalCacheExpiration = LocalCacheExpiration
            };

            var cached = await cache.GetOrCreateAsync(
                cacheKey, 
                async ct =>
                {
                    var providerResult = await providersApi.GetProviderByUserIdAsync(uId, ct);
                    
                    if (providerResult.IsFailure)
                    {
                        throw new UpstreamProviderException(providerResult.Error.Message, providerResult.Error.StatusCode);
                    }

                    if (providerResult.Value == null)
                    {
                        return ProviderResolutionResult.NotLinked();
                    }

                    return ProviderResolutionResult.Found(providerResult.Value.Id);
                },
                options: options,
                cancellationToken: cancellationToken);

            return cached switch
            {
                { IsFound: true, ProviderId: Guid providerId } => ProviderAuthorizationResult.Authorized(uId, providerId),
                { IsNotLinked: true } => ProviderAuthorizationResult.NotLinked(uId),
                _ => LogAndReturnUnauthorized(uId, cached)
            };
        }
        catch (UpstreamProviderException ex)
        {
            logger.LogWarning("Failed to resolve provider for user {UserId}: {Error}", uId, ex.Message);
            return ProviderAuthorizationResult.UpstreamFailure(ex.Message, ex.StatusCode);
        }
    }

    /// <summary>
    /// Autoriza uma operação baseada no Dono (Cliente), Prestador ou Admin.
    /// </summary>
    public async Task<Result> AuthorizeBookingOperationAsync(
        ClaimsPrincipal user,
        Guid? bookingClientId,
        Guid? bookingProviderId,
        CancellationToken cancellationToken = default)
    {
        var authResult = await ResolveAsync(user, cancellationToken);
        return AuthorizeBookingOperation(
            authResult.IsAdmin, 
            authResult.ProviderId, 
            authResult.UserId, 
            bookingClientId, 
            bookingProviderId);
    }

    /// <summary>
    /// Lógica centralizada de autorização para agendamentos.
    /// Pode ser usada em Handlers que recebem dados de autorização via Comando.
    /// </summary>
    public static Result AuthorizeBookingOperation(
        bool isSystemAdmin,
        Guid? userProviderId,
        Guid? userClientId,
        Guid? bookingClientId,
        Guid? bookingProviderId)
    {
        if (isSystemAdmin) return Result.Success();

        // 1. Verificar se é o Dono (Cliente)
        if (bookingClientId.HasValue && userClientId.HasValue && userClientId.Value == bookingClientId.Value)
        {
            return Result.Success();
        }

        // 2. Verificar se é o Prestador
        if (bookingProviderId.HasValue && userProviderId.HasValue && userProviderId.Value == bookingProviderId.Value)
        {
            return Result.Success();
        }

        return Result.Failure(Error.Forbidden("Você não tem permissão para realizar esta operação."));
    }

    private ProviderAuthorizationResult LogAndReturnUnauthorized(Guid userId, ProviderResolutionResult? result)
    {
        logger.LogError("Unexpected ProviderResolutionResult for user {UserId}: {@Result}", userId, result);
        return ProviderAuthorizationResult.Unauthorized("Erro interno ao resolver vínculo do prestador.");
    }
}

[ExcludeFromCodeCoverage]
internal sealed class UpstreamProviderException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

[ExcludeFromCodeCoverage]
internal sealed record ProviderResolutionResult
{
    public Guid? ProviderId { get; init; }
    public bool IsNotLinked { get; init; }

    [JsonIgnore]
    public bool IsFound => ProviderId.HasValue;

    [JsonConstructor]
    public ProviderResolutionResult() { }

    public static ProviderResolutionResult NotLinked() => new() { IsNotLinked = true };
    public static ProviderResolutionResult Found(Guid providerId) => new() { ProviderId = providerId };
}
