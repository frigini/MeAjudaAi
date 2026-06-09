using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Bookings.Application.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public class GetProviderAvailabilityEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/availability/{providerId}", GetProviderAvailabilityAsync)
        .RequireAuthorization()
        .Produces<AvailabilityDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("GetProviderAvailability")
        .WithSummary("Consulta disponibilidade")
        .WithDescription("Consulta a disponibilidade de um prestador em uma data específica.");
    }

    /// <summary>
    /// Consulta a disponibilidade de um prestador em uma data específica.
    /// </summary>
    /// <param name="providerId">ID do prestador.</param>
    /// <param name="date">Data da consulta.</param>
    /// <param name="dispatcher">Disparador de queries.</param>
    /// <param name="context">Contexto da requisição HTTP.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>A disponibilidade do prestador.</returns>
    private static async Task<IResult> GetProviderAvailabilityAsync(
        Guid providerId,
        [FromQuery] DateOnly date,
        [FromServices] IQueryDispatcher dispatcher,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var correlationIdHeader = context.Request.Headers[AuthConstants.Headers.CorrelationId].ToString();
        if (!Guid.TryParse(correlationIdHeader, out var correlationId))
        {
            correlationId = Guid.NewGuid();
        }

        var query = new GetProviderAvailabilityQuery(providerId, date, correlationId);
        var result = await dispatcher.QueryAsync<GetProviderAvailabilityQuery, Result<AvailabilityDto>>(query, cancellationToken);

        return result.Match(
            onSuccess: availability => Results.Ok(availability),
            onFailure: error => Results.Problem(error.Message, statusCode: error.StatusCode)
        );
    }
}
