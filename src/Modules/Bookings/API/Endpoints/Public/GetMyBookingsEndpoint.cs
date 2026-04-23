using System.Security.Claims;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public class GetMyBookingsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/my", async (
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to,
            [FromServices] IQueryDispatcher dispatcher,
            [FromServices] ILogger<GetMyBookingsEndpoint> logger,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var userIdClaim = context.User.FindFirst(AuthConstants.Claims.Subject)?.Value ?? 
                             context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var clientId))
            {
                return Results.Problem("Autenticação necessária.", statusCode: StatusCodes.Status401Unauthorized);
            }

            if (from.HasValue && to.HasValue && from > to)
            {
                return Results.Problem("A data inicial ('from') não pode ser posterior à data final ('to').", statusCode: StatusCodes.Status400BadRequest);
            }

            var correlationIdHeader = context.Request.Headers[AuthConstants.Headers.CorrelationId];
            var correlationIdRaw = correlationIdHeader.FirstOrDefault();
            var correlationId = Guid.TryParse(correlationIdRaw, out var parsedId) ? parsedId : Guid.NewGuid();
            
            if (!string.IsNullOrEmpty(correlationIdRaw) && !Guid.TryParse(correlationIdRaw, out _))
            {
                logger.LogWarning("Failed to parse CorrelationId header '{HeaderKey}': raw value '{RawValue}'. Using new GUID instead.", 
                    AuthConstants.Headers.CorrelationId, correlationIdRaw);
            }

            var query = new GetBookingsByClientQuery(clientId, correlationId, page, pageSize, from?.UtcDateTime, to?.UtcDateTime);
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