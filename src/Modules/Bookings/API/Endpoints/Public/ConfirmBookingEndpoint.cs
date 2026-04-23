using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public class ConfirmBookingEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}/confirm", async (
            Guid id,
            [FromServices] ICommandDispatcher dispatcher,
            ClaimsPrincipal user,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              user.FindFirst(AuthConstants.Claims.Subject)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Forbid();
            }

            var correlationIdHeader = context.Request.Headers[AuthConstants.Headers.CorrelationId].ToString();
            if (!Guid.TryParse(correlationIdHeader, out var correlationId))
            {
                correlationId = Guid.NewGuid();
            }

            var command = new ConfirmBookingCommand(id, correlationId);
            var result = await dispatcher.SendAsync<ConfirmBookingCommand, Result>(command, cancellationToken);

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
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("ConfirmBooking")
        .WithSummary("Confirma um agendamento pendente.");
    }
}
