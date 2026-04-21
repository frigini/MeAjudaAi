using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Commands;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public class CreateBookingEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (
            CreateBookingRequest request,
            [FromServices] ICommandDispatcher dispatcher,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
        {
            var userIdClaim = user.FindFirst(AuthConstants.Claims.Subject)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var clientId))
            {
                return Results.Unauthorized();
            }

            var command = new CreateBookingCommand(
                request.ProviderId,
                clientId,
                request.ServiceId,
                request.Start,
                request.End,
                Guid.NewGuid());

            var result = await dispatcher.SendAsync<CreateBookingCommand, Result<BookingDto>>(command, cancellationToken);

            return result.Match(
                onSuccess: booking => Results.Created($"/api/v1/bookings/{booking.Id}", booking),
                onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
            );
        })
        .RequireAuthorization()
        .Produces<BookingDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("CreateBooking")
        .WithSummary("Cria um novo agendamento.");
    }
}

public record CreateBookingRequest(
    Guid ProviderId,
    Guid ServiceId,
    DateTimeOffset Start,
    DateTimeOffset End);
