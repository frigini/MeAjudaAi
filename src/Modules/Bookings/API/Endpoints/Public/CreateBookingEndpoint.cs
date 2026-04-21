using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public class CreateBookingEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (
            CreateBookingRequest request,
            [FromServices] ICommandDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateBookingCommand(
                request.ProviderId,
                request.ClientId,
                request.ServiceId,
                request.Start,
                request.End);

            var result = await dispatcher.SendAsync<CreateBookingCommand, Result<BookingDto>>(command, cancellationToken);

            return result.Match(
                onSuccess: booking => Results.Created($"/api/v1/bookings/{booking.Id}", booking),
                onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
            );
        })
        .RequireAuthorization()
        .WithTags(BookingsEndpoints.Tag)
        .WithName("CreateBooking")
        .WithSummary("Cria um novo agendamento.");
    }
}

public record CreateBookingRequest(
    Guid ProviderId,
    Guid ClientId,
    Guid ServiceId,
    DateTime Start,
    DateTime End);
