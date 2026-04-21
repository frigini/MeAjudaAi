using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
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
            CancellationToken cancellationToken) =>
        {
            var command = new CancelBookingCommand(id, request.Reason);
            var result = await dispatcher.SendAsync<CancelBookingCommand, Result>(command, cancellationToken);

            return result.Match(
                onSuccess: () => Results.NoContent(),
                onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
            );
        })
        .RequireAuthorization()
        .WithTags(BookingsEndpoints.Tag)
        .WithName("CancelBooking")
        .WithSummary("Cancela um agendamento.");
    }
}

public record CancelBookingRequest(string Reason);
