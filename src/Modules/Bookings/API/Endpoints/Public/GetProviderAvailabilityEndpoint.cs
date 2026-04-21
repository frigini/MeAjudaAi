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

public class GetProviderAvailabilityEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/availability/{providerId}", async (
            Guid providerId,
            [FromQuery] DateOnly date,
            [FromServices] IQueryDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            var query = new GetProviderAvailabilityQuery(providerId, date);
            var result = await dispatcher.QueryAsync<GetProviderAvailabilityQuery, Result<AvailabilityDto>>(query, cancellationToken);

            return result.Match(
                onSuccess: availability => Results.Ok(availability),
                onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
            );
        })
        .RequireAuthorization()
        .Produces<AvailabilityDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("GetProviderAvailability")
        .WithSummary("Consulta a disponibilidade de um prestador em uma data específica.");
    }
}
