using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public class CancelBookingEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}/cancel", async (
            Guid id,
            CancelBookingRequest request,
            [FromServices] ICommandDispatcher dispatcher,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var correlationIdHeader = context.Request.Headers[AuthConstants.Headers.CorrelationId].ToString();
            if (!Guid.TryParse(correlationIdHeader, out var correlationId))
            {
                correlationId = Guid.NewGuid();
            }

            var command = new CancelBookingCommand(id, request.Reason, correlationId);
            var result = await dispatcher.SendAsync<CancelBookingCommand, Result>(command, cancellationToken);

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
        .WithName("CancelBooking")
        .WithSummary("Cancela um agendamento.");
    }
}

public record CancelBookingRequest(string Reason);
