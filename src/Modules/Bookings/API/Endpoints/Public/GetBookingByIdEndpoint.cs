using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
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
            CancellationToken cancellationToken) =>
        {
            var query = new GetBookingByIdQuery(id, Guid.NewGuid());
            var result = await dispatcher.QueryAsync<GetBookingByIdQuery, Result<BookingDto>>(query, cancellationToken);

            return result.Match(
                onSuccess: booking => Results.Ok(booking),
                onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
            );
        })
        .RequireAuthorization()
        .Produces<BookingDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("GetBookingById")
        .WithSummary("Obtém os detalhes de um agendamento pelo ID.");
    }
}
