using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public class GetMyBookingsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/my", async (
            [FromServices] IQueryDispatcher dispatcher,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var userIdClaim = context.User.FindFirst(AuthConstants.Claims.Subject)?.Value ?? 
                             context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var clientId))
            {
                return Results.Unauthorized();
            }

            var query = new GetBookingsByClientQuery(clientId, Guid.NewGuid());
            var result = await dispatcher.QueryAsync<GetBookingsByClientQuery, Result<IReadOnlyList<BookingDto>>>(query, cancellationToken);

            return result.Match(
                onSuccess: bookings => Results.Ok(bookings),
                onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
            );
        })
        .RequireAuthorization()
        .Produces<IReadOnlyList<BookingDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("GetMyBookings")
        .WithSummary("Lista os agendamentos do cliente autenticado.");
    }
}
