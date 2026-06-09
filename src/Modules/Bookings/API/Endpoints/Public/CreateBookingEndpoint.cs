using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Commands;
using MeAjudaAi.Modules.Bookings.Application.DTOs;
using MeAjudaAi.Modules.Bookings.Application.DTOs.Requests;
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
        app.MapPost("", CreateBookingAsync)
        .RequireAuthorization()
        .Produces<BookingDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("CreateBooking")
        .WithSummary("Cria um novo agendamento.");
    }

    /// <summary>
    /// Cria um novo agendamento de serviço.
    /// </summary>
    /// <param name="request">Dados do agendamento (prestador, serviço, datas).</param>
    /// <param name="dispatcher">Disparador de comandos.</param>
    /// <param name="context">Contexto da requisição HTTP para extração de identidade.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <returns>O agendamento criado com status 201.</returns>
    private static async Task<IResult> CreateBookingAsync(
        CreateBookingRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userIdClaim = context.User.FindFirst(AuthConstants.Claims.Subject)?.Value ?? 
                         context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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
    }
}
