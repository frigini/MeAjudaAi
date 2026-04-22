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

public class GetProviderBookingsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/provider/{providerId}", async (
            Guid providerId,
            [FromServices] IQueryDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            var query = new GetBookingsByProviderQuery(providerId, Guid.NewGuid());
            var result = await dispatcher.QueryAsync<GetBookingsByProviderQuery, Result<IReadOnlyList<BookingDto>>>(query, cancellationToken);

            return result.Match(
                onSuccess: bookings => Results.Ok(bookings),
                onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
            );
        })
        .RequireAuthorization()
        .Produces<IReadOnlyList<BookingDto>>(StatusCodes.Status200OK)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("GetProviderBookings")
        .WithSummary("Lista os agendamentos de um prestador.");
    }
}
