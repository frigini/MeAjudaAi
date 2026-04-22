using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
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
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromServices] IQueryDispatcher dispatcher,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var userIdClaim = context.User.FindFirst(AuthConstants.Claims.Subject)?.Value ?? 
                             context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var clientId))
            {
                return Results.Problem("Autenticação necessária.", statusCode: StatusCodes.Status401Unauthorized);
            }

            var correlationIdHeader = context.Request.Headers["X-Correlation-Id"].ToString();
            var correlationId = Guid.TryParse(correlationIdHeader, out var parsedId) ? parsedId : Guid.NewGuid();

            var query = new GetBookingsByClientQuery(clientId, correlationId, page, pageSize, from, to);
            var result = await dispatcher.QueryAsync<GetBookingsByClientQuery, Result<PagedResult<BookingDto>>>(query, cancellationToken);

            return result.Match(
                onSuccess: bookings => Results.Ok(bookings),
                onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
            );
        })
        .RequireAuthorization()
        .Produces<PagedResult<BookingDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("GetMyBookings")
        .WithSummary("Lista os agendamentos do cliente autenticado com paginação e filtros.");
    }
}
