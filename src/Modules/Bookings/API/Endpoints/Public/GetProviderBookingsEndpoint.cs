using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Bookings.Application.DTOs;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Modules.Bookings.Application.Common;
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
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/provider/{providerId}", GetProviderBookingsAsync)
        .RequireAuthorization()
        .Produces<PagedResult<BookingDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("GetProviderBookings")
        .WithSummary("Lista agendamentos do prestador")
        .WithDescription("Retorna uma lista paginada de agendamentos para um prestador específico.");
    }

    /// <summary>
    /// Lista os agendamentos de um prestador com paginação e filtros opcionais.
    /// </summary>
    /// <param name="providerId">ID do prestador.</param>
    /// <param name="page">Número da página.</param>
    /// <param name="pageSize">Quantidade de itens por página.</param>
    /// <param name="from">Data inicial do filtro.</param>
    /// <param name="to">Data final do filtro.</param>
    /// <param name="dispatcher">Disparador de queries.</param>
    /// <param name="authResolver">Resolvedor de autorização do prestador.</param>
    /// <param name="logger">Logger da aplicação.</param>
    /// <param name="context">Contexto da requisição HTTP.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Uma lista paginada de agendamentos.</returns>
    private static async Task<IResult> GetProviderBookingsAsync(
        Guid providerId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromServices] IQueryDispatcher dispatcher,
        [FromServices] ProviderAuthorizationResolver authResolver,
        [FromServices] ILogger<GetProviderBookingsEndpoint> logger,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        if (providerId == Guid.Empty)
        {
            return Results.Problem("ProviderId não pode ser vazio.", statusCode: StatusCodes.Status400BadRequest);
        }

        var normalizedPage = page ?? Pagination.DefaultPageNumber;
        if (normalizedPage < Pagination.MinPageSize)
        {
            return Results.Problem($"O parâmetro 'page' deve ser maior ou igual a {Pagination.MinPageSize}.", statusCode: StatusCodes.Status400BadRequest);
        }

        var normalizedPageSize = pageSize ?? Pagination.DefaultPageSize;
        if (normalizedPageSize is < Pagination.MinPageSize or >Pagination.MaxPageSize)
        {
            return Results.Problem($"O parâmetro 'pageSize' deve estar entre {Pagination.MinPageSize} e {Pagination.MaxPageSize}.", statusCode: StatusCodes.Status400BadRequest);
        }

        var authResult = await authResolver.ResolveAsync(context.User, cancellationToken);

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
    }
}