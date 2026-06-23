using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

[ExcludeFromCodeCoverage]
public class GetBookingByIdEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Bookings.GetById, GetBookingByIdAsync)
        .RequireAuthorization()
        .Produces<ModuleBookingDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("GetBookingById")
        .WithSummary("Obtém agendamento")
        .WithDescription("Obtém os detalhes de um agendamento pelo ID.");
    }

    /// <summary>
    /// Obtém os detalhes completos de um agendamento específico.
    /// </summary>
    /// <param name="id">ID do agendamento.</param>
    /// <param name="dispatcher">Disparador de queries.</param>
    /// <param name="context">Contexto da requisição HTTP para extração de identidade.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Detalhes do agendamento.</returns>
    private static async Task<IResult> GetBookingByIdAsync(
        Guid id,
        [FromServices] IQueryDispatcher dispatcher,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var correlationIdHeader = context.Request.Headers[AuthConstants.Headers.CorrelationId].FirstOrDefault();
        var correlationId = Guid.TryParse(correlationIdHeader, out var cId) ? cId : Guid.NewGuid();

        var user = context.User;
        var userId = Guid.TryParse(user.FindFirst(AuthConstants.Claims.Subject)?.Value, out var uId) ? uId : (Guid?)null;
        var providerId = Guid.TryParse(user.FindFirst(AuthConstants.Claims.ProviderId)?.Value, out var pId) ? pId : (Guid?)null;
        var isSystemAdmin = string.Equals(user.FindFirst(AuthConstants.Claims.IsSystemAdmin)?.Value, "true", StringComparison.OrdinalIgnoreCase);

        var query = new GetBookingByIdQuery(id, userId, providerId, isSystemAdmin, correlationId);
        var result = await dispatcher.QueryAsync<GetBookingByIdQuery, Result<ModuleBookingDto>>(query, cancellationToken);

        return result.Match(
            onSuccess: booking => Results.Ok(booking),
            onFailure: error => error.ToProblem()
        );
    }
}
