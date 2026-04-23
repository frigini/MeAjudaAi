using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public class GetProviderBookingsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/provider/{providerId}", async (
            Guid providerId,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromServices] IQueryDispatcher dispatcher,
            [FromServices] IProvidersModuleApi providersApi,
            [FromServices] IMemoryCache cache,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var user = context.User;
            var providerIdClaim = user.FindFirst(AuthConstants.Claims.ProviderId)?.Value;
            var isSystemAdmin = string.Equals(user.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, "true", StringComparison.OrdinalIgnoreCase);

            if (!isSystemAdmin)
            {
                bool isAuthorized = false;
                if (!string.IsNullOrEmpty(providerIdClaim) && Guid.TryParse(providerIdClaim, out var pId))
                {
                    isAuthorized = pId == providerId;
                }
                else
                {
                    var userIdClaim = user.FindFirst(AuthConstants.Claims.Subject)?.Value;
                    if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var uId))
                    {
                        var cacheKey = $"provider_id_user_{uId}";
                        if (!cache.TryGetValue(cacheKey, out Guid cachedProviderId))
                        {
                            var providerResult = await providersApi.GetProviderByUserIdAsync(uId, cancellationToken);
                            if (providerResult.IsSuccess && providerResult.Value != null)
                            {
                                cachedProviderId = providerResult.Value.Id;
                                cache.Set(cacheKey, cachedProviderId, TimeSpan.FromMinutes(30));
                            }
                        }
                        isAuthorized = cachedProviderId == providerId;
                    }
                }

                if (!isAuthorized)
                {
                    return Results.Forbid();
                }
            }

            var correlationIdHeader = context.Request.Headers[AuthConstants.Headers.CorrelationId].FirstOrDefault();
            var correlationId = Guid.TryParse(correlationIdHeader, out var cId) ? cId : Guid.NewGuid();

            var query = new GetBookingsByProviderQuery(providerId, correlationId, page, pageSize);
            var result = await dispatcher.QueryAsync<GetBookingsByProviderQuery, Result<PagedResult<BookingDto>>>(query, cancellationToken);

            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : Results.Problem(result.Error.Message, statusCode: result.Error.StatusCode);
        })
        .RequireAuthorization()
        .Produces<PagedResult<BookingDto>>(StatusCodes.Status200OK)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("GetProviderBookings")
        .WithSummary("Lista os agendamentos de um prestador.");
    }
}
