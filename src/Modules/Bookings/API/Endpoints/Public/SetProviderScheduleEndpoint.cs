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

public sealed class ProviderAuthorizationResult
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

public static class ProviderAuthorizationResultExtensions
{
    public static IResult? ToProblemResult(this ProviderAuthorizationResult result)
    {
        return result.FailureKind switch
        {
            AuthorizationFailureKind.UpstreamFailure => 
                Results.Problem(result.ErrorMessage, statusCode: result.ErrorStatusCode ?? StatusCodes.Status500InternalServerError),
            AuthorizationFailureKind.Unauthorized => 
                Results.Problem(result.ErrorMessage ?? "Acesso não autorizado.", statusCode: StatusCodes.Status401Unauthorized),
            AuthorizationFailureKind.NotLinked => 
                Results.Problem("Usuário não possui prestador vinculado.", statusCode: StatusCodes.Status404NotFound),
            _ => null
        };
    }
}

public sealed class ProviderAuthorizationResolver
{
    private const string CacheKeyPrefix = "bookings:provider_by_user:";
    private static readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan AbsoluteExpiration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan MissExpiration = TimeSpan.FromMinutes(2);

    private readonly IMemoryCache _cache;
    private readonly ILogger<ProviderAuthorizationResolver> _logger;

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
        if (!string.IsNullOrEmpty(providerIdClaim) && Guid.TryParse(providerIdClaim, out var pId) && pId != Guid.Empty)
        {
            return ProviderAuthorizationResult.Authorized(pId);
        }

        var userIdClaim = user.FindFirst(AuthConstants.Claims.Subject)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var uId))
        {
            return ProviderAuthorizationResult.Unauthorized("Identificação do usuário não encontrada.");
        }

        var cacheKey = $"{CacheKeyPrefix}{uId}";

        try
        {
            var cached = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SlidingExpiration = SlidingExpiration;
                entry.AbsoluteExpirationRelativeToNow = AbsoluteExpiration;
                
                var providerResult = await providersApi.GetProviderByUserIdAsync(uId, cancellationToken);
                
                if (providerResult.IsFailure)
                {
                    throw new UpstreamProviderException(providerResult.Error.Message, providerResult.Error.StatusCode);
                }

                if (providerResult.Value == null)
                {
                    entry.AbsoluteExpirationRelativeToNow = MissExpiration;
                    return ProviderResolutionResult.NotLinked();
                }

                return ProviderResolutionResult.Found(providerResult.Value.Id);
            });

            return cached switch
            {
                { IsFound: true } => ProviderAuthorizationResult.Authorized(cached.ProviderId!.Value),
                _ => ProviderAuthorizationResult.NotLinked()
            };
        }
        catch (UpstreamProviderException ex)
        {
            _logger.LogWarning("Failed to resolve provider for user {UserId}: {Error}", uId, ex.Message);
            return ProviderAuthorizationResult.UpstreamFailure(ex.Message, ex.StatusCode);
        }
    }
}

internal sealed class UpstreamProviderException : Exception
{
    public int StatusCode { get; }
    public UpstreamProviderException(string message, int statusCode) : base(message) => StatusCode = statusCode;
}

internal sealed class ProviderResolutionResult
{
    public Guid? ProviderId { get; init; }
    public bool IsNotLinked { get; init; }
    public bool IsFound => ProviderId.HasValue;

    private ProviderResolutionResult() { }

    public static ProviderResolutionResult NotLinked() => new() { IsNotLinked = true };
    public static ProviderResolutionResult Found(Guid providerId) => new() { ProviderId = providerId };
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
            if (request == null)
            {
                return Results.Problem("Corpo da requisição é obrigatório.", statusCode: StatusCodes.Status400BadRequest);
            }

            if (request.Availabilities == null)
            {
                return Results.Problem("Propriedade 'Availabilities' é obrigatória.", statusCode: StatusCodes.Status400BadRequest);
            }

            if (!request.Availabilities.Any())
            {
                return Results.Problem("A lista de disponibilidades não pode ser vazia.", statusCode: StatusCodes.Status400BadRequest);
            }

            var authResult = await authResolver.ResolveAsync(context, providersApi, cancellationToken);

            var authError = authResult.ToProblemResult();
            if (authError != null)
            {
                return authError;
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
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("SetProviderSchedule")
        .WithSummary("Define a agenda de horários de trabalho de um prestador.");
    }
}

public record SetProviderScheduleRequest(
    Guid ProviderId,
    IEnumerable<ProviderScheduleDto> Availabilities);