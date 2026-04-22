using MeAjudaAi.Contracts.Functional;
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

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public class GetProviderBookingsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/provider/{providerId}", async (
            Guid providerId,
            [FromServices] IQueryDispatcher dispatcher,
            [FromServices] IProvidersModuleApi providersApi,
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
                        var providerResult = await providersApi.GetProviderByUserIdAsync(uId, cancellationToken);
                        isAuthorized = providerResult.IsSuccess && providerResult.Value != null && providerResult.Value.Id == providerId;
                    }
                }

                if (!isAuthorized)
                {
                    return Results.Forbid();
                }
            }

            var query = new GetBookingsByProviderQuery(providerId, Guid.NewGuid());
            var result = await dispatcher.QueryAsync<GetBookingsByProviderQuery, Result<IReadOnlyList<BookingDto>>>(query, cancellationToken);

            return result.Match(
                onSuccess: bookings => Results.Ok(bookings),
                onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
            );
        })
        .RequireAuthorization()
        .Produces<IReadOnlyList<BookingDto>>(StatusCodes.Status200OK)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("GetProviderBookings")
        .WithSummary("Lista os agendamentos de um prestador.");
    }
}
