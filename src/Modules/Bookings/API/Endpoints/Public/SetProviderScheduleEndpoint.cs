using System.Collections.Concurrent;
using System.Security.Claims;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public enum AuthorizationFailureKind
{
    None,
    Unauthorized,
    UpstreamFailure,
    NotLinked
}

public class ProviderAuthorizationResult
{
    public bool IsAdmin { get; init; }
    public Guid? ProviderId { get; init; }
    public AuthorizationFailureKind FailureKind { get; init; }
    public string? ErrorMessage { get; init; }
    public int? ErrorStatusCode { get; init; }

    public static ProviderAuthorizationResult Admin() => new() { IsAdmin = true };
    public static ProviderAuthorizationResult Authorized(Guid providerId) => new() { ProviderId = providerId };
    public static ProviderAuthorizationResult NotLinked() => new() { FailureKind = AuthorizationFailureKind.NotLinked };
    public static ProviderAuthorizationResult Unauthorized(string? message = null) => 
        new() { FailureKind = AuthorizationFailureKind.Unauthorized, ErrorMessage = message };
    public static ProviderAuthorizationResult UpstreamFailure(string message, int statusCode) => 
        new() { FailureKind = AuthorizationFailureKind.UpstreamFailure, ErrorMessage = message, ErrorStatusCode = statusCode };
}

public class ProviderAuthorizationResolver
{
    private const string CacheKeyPrefix = "bookings:provider_by_user:";
    private static readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan AbsoluteExpiration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan MissExpiration = TimeSpan.FromMinutes(2);

    private readonly IMemoryCache _cache;
    private readonly ILogger<ProviderAuthorizationResolver> _logger;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

    public ProviderAuthorizationResolver(IMemoryCache cache, ILogger<ProviderAuthorizationResolver> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<ProviderAuthorizationResult> ResolveAsync(
        HttpContext httpContext,
        IProvidersModuleApi providersApi,
        CancellationToken cancellationToken = default)
    {
        var user = httpContext.User;
        var isSystemAdmin = string.Equals(user.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, "true", StringComparison.OrdinalIgnoreCase);

        if (isSystemAdmin)
        {
            return ProviderAuthorizationResult.Admin();
        }

        var providerIdClaim = user.FindFirst(AuthConstants.Claims.ProviderId)?.Value;
        if (!string.IsNullOrEmpty(providerIdClaim) && Guid.TryParse(providerIdClaim, out var pId))
        {
            return ProviderAuthorizationResult.Authorized(pId);
        }

        var userIdClaim = user.FindFirst(AuthConstants.Claims.Subject)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var uId))
        {
            return ProviderAuthorizationResult.Unauthorized("Identificação do usuário não encontrada.");
        }

        var cacheKey = $"{CacheKeyPrefix}{uId}";

        var semaphore = _semaphores.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue(cacheKey, out Guid cachedProviderId))
            {
                if (cachedProviderId == Guid.Empty)
                {
                    _logger.LogDebug("Cached miss for user {UserId}", uId);
                    return ProviderAuthorizationResult.NotLinked();
                }
                return ProviderAuthorizationResult.Authorized(cachedProviderId);
            }

            var providerResult = await providersApi.GetProviderByUserIdAsync(uId, cancellationToken);
            
            if (providerResult.IsFailure)
            {
                _logger.LogWarning("Failed to resolve provider for user {UserId}: {Error}", uId, providerResult.Error.Message);
                return ProviderAuthorizationResult.UpstreamFailure(providerResult.Error.Message, providerResult.Error.StatusCode);
            }

            if (providerResult.Value == null)
            {
                _cache.Set(cacheKey, Guid.Empty, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = MissExpiration
                });
                _logger.LogDebug("User {UserId} has no associated provider (cached)", uId);
                return ProviderAuthorizationResult.NotLinked();
            }

            var resolvedProviderId = providerResult.Value.Id;
            _cache.Set(cacheKey, resolvedProviderId, new MemoryCacheEntryOptions
            {
                SlidingExpiration = SlidingExpiration,
                AbsoluteExpirationRelativeToNow = AbsoluteExpiration
            });
            _logger.LogDebug("Resolved provider {ProviderId} for user {UserId}", resolvedProviderId, uId);
            
            return ProviderAuthorizationResult.Authorized(resolvedProviderId);
        }
        finally
        {
            semaphore.Release();
            if (_semaphores.TryRemove(cacheKey, out var removed))
            {
                removed.Dispose();
            }
        }
    }
}

public class SetProviderScheduleEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/schedule", async (
            SetProviderScheduleRequest request,
            [FromServices] ICommandDispatcher dispatcher,
            [FromServices] IProvidersModuleApi providersApi,
            [FromServices] ProviderAuthorizationResolver authResolver,
            [FromServices] ILogger<SetProviderScheduleEndpoint> logger,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            if (request == null || request.Availabilities == null || !request.Availabilities.Any())
            {
                return Results.Problem("A lista de disponibilidades não pode ser vazia.", statusCode: StatusCodes.Status400BadRequest);
            }

            var authResult = await authResolver.ResolveAsync(context, providersApi, cancellationToken);

            if (authResult.FailureKind == AuthorizationFailureKind.UpstreamFailure)
            {
                return Results.Problem(authResult.ErrorMessage, statusCode: authResult.ErrorStatusCode ?? StatusCodes.Status500InternalServerError);
            }

            if (authResult.FailureKind == AuthorizationFailureKind.Unauthorized)
            {
                return Results.Problem(authResult.ErrorMessage ?? "Acesso não autorizado.", statusCode: StatusCodes.Status401Unauthorized);
            }

            if (authResult.FailureKind == AuthorizationFailureKind.NotLinked)
            {
                return Results.Problem("Usuário não possui prestador vinculado.", statusCode: StatusCodes.Status404NotFound);
            }

            Guid targetProviderId;

            if (authResult.IsAdmin)
            {
                if (request.ProviderId == Guid.Empty)
                {
                    return Results.Problem("ProviderId inválido para operação admin.", statusCode: StatusCodes.Status400BadRequest);
                }
                targetProviderId = request.ProviderId;
                var userIdClaim = context.User.FindFirst(AuthConstants.Claims.Subject)?.Value;
                logger.LogInformation("Admin {AdminId} is setting schedule for Provider {ProviderId}", userIdClaim, targetProviderId);
            }
            else
            {
                targetProviderId = authResult.ProviderId!.Value;

                if (request.ProviderId != Guid.Empty && request.ProviderId != targetProviderId)
                {
                    return Results.Problem("O ProviderId informado não coincide com o prestador autenticado.", statusCode: StatusCodes.Status400BadRequest);
                }

                logger.LogInformation("Provider {ProviderId} is setting own schedule", targetProviderId);
            }

            var correlationIdHeader = context.Request.Headers[AuthConstants.Headers.CorrelationId].FirstOrDefault();
            var correlationId = Guid.TryParse(correlationIdHeader, out var parsedId) ? parsedId : Guid.NewGuid();

            var command = new SetProviderScheduleCommand(
                targetProviderId,
                request.Availabilities,
                correlationId);

            var result = await dispatcher.SendAsync<SetProviderScheduleCommand, Result>(command, cancellationToken);

            return result.Match(
                onSuccess: () => Results.NoContent(),
                onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
            );
        })
        .RequireAuthorization()
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("SetProviderSchedule")
        .WithSummary("Define a agenda de horários de trabalho de um prestador.");
    }
}

public record SetProviderScheduleRequest(
    Guid ProviderId,
    IEnumerable<ProviderScheduleDto> Availabilities);