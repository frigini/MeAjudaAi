using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Modules.Bookings.Application.Authorization.Models;
using MeAjudaAi.Modules.Bookings.Application.Enums;
using MeAjudaAi.Modules.Bookings.Domain.Exceptions;
using MeAjudaAi.Shared.Caching;
using MeAjudaAi.Shared.Resources;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Bookings.Application.Authorization;

public sealed class ProviderAuthorizationResolver(
    ICacheService cache,
    IProvidersModuleApi providersApi,
    ILogger<ProviderAuthorizationResolver> logger,
    IStringLocalizer<Strings> localizer)
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
            return ProviderAuthorizationResult.Unauthorized(localizer["UserIdNotFoundOrInvalid"]);
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
        
        if (authResult.FailureKind is not EAuthorizationFailureKind.None and not EAuthorizationFailureKind.NotLinked)
        {
            return authResult.FailureKind switch
            {
                EAuthorizationFailureKind.Unauthorized => Result.Failure(Error.Unauthorized(authResult.ErrorMessage ?? localizer["UnauthorizedAccess"])),
                EAuthorizationFailureKind.UpstreamFailure => Result.Failure(new Error(authResult.ErrorMessage ?? localizer["ProviderValidationError"], authResult.ErrorStatusCode ?? 502)),
                _ => Result.Failure(Error.Forbidden(authResult.ErrorMessage ?? localizer["AccessDenied"]))
            };
        }

        return AuthorizeBookingOperation(
            authResult.IsAdmin, 
            authResult.ProviderId, 
            authResult.UserId, 
            bookingClientId, 
            bookingProviderId,
            localizer);
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
        Guid? bookingProviderId,
        IStringLocalizer<Strings> localizer)
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

        return Result.Failure(Error.Forbidden(localizer["Unauthorized"]));
    }

    private ProviderAuthorizationResult LogAndReturnUnauthorized(Guid userId, ProviderResolutionResult? result)
    {
        logger.LogError("Unexpected ProviderResolutionResult for user {UserId}: {@Result}", userId, result);
        return ProviderAuthorizationResult.Unauthorized(localizer["ProviderBindingInternalError"]);
    }
}
