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

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public class GetBookingByIdEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}", async (
            Guid id,
            [FromServices] IQueryDispatcher dispatcher,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var correlationIdHeader = context.Request.Headers["X-Correlation-Id"].ToString();
            if (!Guid.TryParse(correlationIdHeader, out var correlationId))
            {
                correlationId = Guid.NewGuid();
            }

            var user = context.User;
            var userId = Guid.TryParse(user.FindFirst(AuthConstants.Claims.Subject)?.Value, out var uId) ? uId : (Guid?)null;
            var providerId = Guid.TryParse(user.FindFirst(AuthConstants.Claims.ProviderId)?.Value, out var pId) ? pId : (Guid?)null;
            var isSystemAdmin = string.Equals(user.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, "true", StringComparison.OrdinalIgnoreCase);

            var query = new GetBookingByIdQuery(id, userId, providerId, isSystemAdmin, correlationId);
            var result = await dispatcher.QueryAsync<GetBookingByIdQuery, Result<BookingDto>>(query, cancellationToken);

            return result.Match(
                onSuccess: booking => Results.Ok(booking),
                onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
            );
        })
        .RequireAuthorization()
        .Produces<BookingDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("GetBookingById")
        .WithSummary("Obtém os detalhes de um agendamento pelo ID.");
    }
}
