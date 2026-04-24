using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Modules.Bookings.Application.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Bookings.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

public class GetProviderBookingsEndpoint : IEndpoint
{
    private const int MaxPageSize = 100;
    
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/provider/{providerId}", async (
            Guid providerId,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromServices] IQueryDispatcher dispatcher,
            [FromServices] IProvidersModuleApi providersApi,
            [FromServices] ProviderAuthorizationResolver authResolver,
            [FromServices] ILogger<GetProviderBookingsEndpoint> logger,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            if (providerId == Guid.Empty)
            {
                return Results.Problem("ProviderId não pode ser vazio.", statusCode: StatusCodes.Status400BadRequest);
            }

            var normalizedPage = Math.Max(1, page ?? 1);
            var normalizedPageSize = pageSize.HasValue 
                ? Math.Clamp(pageSize.Value, 1, MaxPageSize) 
                : 10;

            var authResult = await authResolver.ResolveAsync(context, providersApi, cancellationToken);

            var authError = authResult.ToProblemResult();
            if (authError != null)
            {
                return authError;
            }

            if (!authResult.IsAdmin && authResult.ProviderId.HasValue && authResult.ProviderId.Value != providerId)
            {
                return Results.Problem("Forbidden: provider mismatch", statusCode: StatusCodes.Status403Forbidden);
            }

            var correlationIdHeader = context.Request.Headers[AuthConstants.Headers.CorrelationId].FirstOrDefault();
            var correlationId = Guid.TryParse(correlationIdHeader, out var cId) ? cId : Guid.NewGuid();

            var query = new GetBookingsByProviderQuery(providerId, correlationId, normalizedPage, normalizedPageSize, from, to);
            var result = await dispatcher.QueryAsync<GetBookingsByProviderQuery, Result<PagedResult<BookingDto>>>(query, cancellationToken);

            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : Results.Problem(result.Error.Message, statusCode: result.Error.StatusCode);
        })
        .RequireAuthorization()
        .Produces<PagedResult<BookingDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("GetProviderBookings")
        .WithSummary("Lista os agendamentos de um prestador.");
    }
}