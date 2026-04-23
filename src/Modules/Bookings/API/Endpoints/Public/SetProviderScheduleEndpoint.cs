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

public class SetProviderScheduleEndpoint : IEndpoint
{
    private const string CacheKeyPrefix = "bookings:provider_by_user:";
    
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/schedule", async (
            SetProviderScheduleRequest request,
            [FromServices] ICommandDispatcher dispatcher,
            [FromServices] IProvidersModuleApi providersApi,
            [FromServices] IMemoryCache cache,
            [FromServices] ILogger<SetProviderScheduleEndpoint> logger,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            if (request == null || request.Availabilities == null)
            {
                return Results.Problem("Corpo da requisição ou disponibilidades ausentes.", statusCode: StatusCodes.Status400BadRequest);
            }

            var user = context.User;
            var userIdClaim = user.FindFirst(AuthConstants.Claims.Subject)?.Value
                           ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var providerIdClaim = user.FindFirst(AuthConstants.Claims.ProviderId)?.Value;
            var isSystemAdmin = string.Equals(user.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, "true", StringComparison.OrdinalIgnoreCase);

            Guid targetProviderId;

            if (isSystemAdmin)
            {
                if (request.ProviderId == Guid.Empty)
                {
                    return Results.Problem("ProviderId inválido para operação admin.", statusCode: StatusCodes.Status400BadRequest);
                }
                targetProviderId = request.ProviderId;
                logger.LogInformation("Admin {AdminId} is setting schedule for Provider {ProviderId}", userIdClaim, targetProviderId);
            }
            else if (!string.IsNullOrEmpty(providerIdClaim) && Guid.TryParse(providerIdClaim, out var pId))
            {
                targetProviderId = pId;
                logger.LogInformation("Provider {ProviderId} is setting own schedule", targetProviderId);
            }
            else if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var uId))
            {
                var cacheKey = $"{CacheKeyPrefix}{uId}";
                if (cache.TryGetValue(cacheKey, out Guid cachedProviderId))
                {
                    targetProviderId = cachedProviderId;
                    logger.LogInformation("Resolved provider {ProviderId} from cache for user {UserId}", targetProviderId, userIdClaim);
                }
                else
                {
                    var providerResult = await providersApi.GetProviderByUserIdAsync(uId, cancellationToken);
                    
                    if (providerResult.IsFailure)
                    {
                        logger.LogWarning("Failed to resolve provider for user {UserId}: {Error}", userIdClaim, providerResult.Error.Message);
                        return Results.Problem(providerResult.Error.Message, statusCode: providerResult.Error.StatusCode);
                    }

                    if (providerResult.Value == null)
                    {
                        logger.LogWarning("User {UserId} has no associated provider", userIdClaim);
                        return Results.NotFound("Usuário não possui prestador vinculado.");
                    }

                    targetProviderId = providerResult.Value.Id;
                    cache.Set(cacheKey, targetProviderId, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(5),
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                    });
                    logger.LogInformation("Resolved provider {ProviderId} for user {UserId}", targetProviderId, userIdClaim);
                }
            }
            else
            {
                logger.LogWarning("Missing/invalid claims for user authentication");
                return Results.Unauthorized();
            }

            if (targetProviderId == Guid.Empty)
            {
                return Results.Problem("ProviderId inválido ou ausente.", statusCode: StatusCodes.Status400BadRequest);
            }

            if (!isSystemAdmin && request.ProviderId != Guid.Empty && request.ProviderId != targetProviderId)
            {
                return Results.Problem("O ProviderId informado não coincide com o prestador autenticado.", statusCode: StatusCodes.Status400BadRequest);
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
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("SetProviderSchedule")
        .WithSummary("Define a agenda de horários de trabalho de um prestador.");
    }
}

public record SetProviderScheduleRequest(
    Guid ProviderId,
    IEnumerable<ProviderScheduleDto> Availabilities);