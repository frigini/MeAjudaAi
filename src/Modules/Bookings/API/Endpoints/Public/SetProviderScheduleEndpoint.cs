using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json.Serialization;
using FluentValidation;
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
using MeAjudaAi.Shared.Caching;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

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

[ExcludeFromCodeCoverage]
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
    // TTL para o cache local em memória (L1)
    private static readonly TimeSpan LocalCacheExpiration = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan AbsoluteExpiration = TimeSpan.FromMinutes(5);

    private readonly ICacheService _cache;
    private readonly ILogger<ProviderAuthorizationResolver> _logger;

    public ProviderAuthorizationResolver(ICacheService cache, ILogger<ProviderAuthorizationResolver> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Invalida o cache do usuário especificado.
    /// Chamado por handlers de eventos de integração quando o vínculo muda.
    /// </summary>
    public async Task InvalidateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{userId}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);
        _logger.LogInformation("Cache invalidated for user {UserId}", userId);
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
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return ProviderAuthorizationResult.Unauthorized("Identificação do usuário não encontrada.");
        }

        if (!Guid.TryParse(userIdClaim, out var uId))
        {
            return ProviderAuthorizationResult.Unauthorized("Identificador do usuário inválido.");
        }

        var cacheKey = $"{CacheKeyPrefix}{uId}";

        try
        {
            var options = new HybridCacheEntryOptions
            {
                Expiration = AbsoluteExpiration,
                LocalCacheExpiration = LocalCacheExpiration
            };

            var cached = await _cache.GetOrCreateAsync(
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
                { IsFound: true, ProviderId: Guid providerId } => ProviderAuthorizationResult.Authorized(providerId),
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

[ExcludeFromCodeCoverage]
internal sealed class UpstreamProviderException : Exception
{
    public int StatusCode { get; }
    public UpstreamProviderException(string message, int statusCode) : base(message) => StatusCode = statusCode;
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

public class SetProviderScheduleEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/schedule", async (
            SetProviderScheduleRequest request,
            [FromServices] ICommandDispatcher dispatcher,
            [FromServices] IProvidersModuleApi providersApi,
            [FromServices] ProviderAuthorizationResolver authResolver,
            [FromServices] IValidator<SetProviderScheduleRequest> validator,
            [FromServices] ILogger<SetProviderScheduleEndpoint> logger,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            if (request == null)
            {
                return Results.Problem("Corpo da requisição é obrigatório.", statusCode: StatusCodes.Status400BadRequest);
            }

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
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
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Results.Problem("Identificador do administrador não encontrado no token.", statusCode: StatusCodes.Status400BadRequest);
                }
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
            Guid correlationId;

            if (!string.IsNullOrEmpty(correlationIdHeader) && Guid.TryParse(correlationIdHeader, out var parsedId))
            {
                correlationId = parsedId;
            }
            else
            {
                if (!string.IsNullOrEmpty(correlationIdHeader))
                {
                    var maskedHeader = correlationIdHeader.Length > 4 
                        ? $"...{correlationIdHeader[^4..]}" 
                        : correlationIdHeader;
                    logger.LogDebug("Invalid X-Correlation-Id header received: {CorrelationId}. Generating new one.", maskedHeader);
                }
                correlationId = Guid.NewGuid();
            }

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
        .ProducesProblem(StatusCodes.Status502BadGateway)
        .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
        .ProducesProblem(StatusCodes.Status504GatewayTimeout)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("SetProviderSchedule")
        .WithSummary("Define a agenda de horários de trabalho de um prestador.");
    }
}