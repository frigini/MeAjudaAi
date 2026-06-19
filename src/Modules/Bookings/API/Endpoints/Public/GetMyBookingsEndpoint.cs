using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Bookings.DTOs;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Bookings.Application.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Bookings.API.Endpoints.Public;

[ExcludeFromCodeCoverage]
public class GetMyBookingsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Bookings.GetMy, GetMyBookingsAsync)
        .RequireAuthorization()
        .Produces<PagedResult<ModuleBookingDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithTags(BookingsEndpoints.Tag)
        .WithName("GetMyBookings")
        .WithSummary("Lista agendamentos do cliente")
        .WithDescription("Retorna uma lista paginada de agendamentos realizados pelo cliente autenticado, com suporte a filtros de data.");
    }

    /// <summary>
    /// Lista os agendamentos do cliente autenticado com paginação e filtros opcionais.
    /// </summary>
    /// <param name="page">Número da página.</param>
    /// <param name="pageSize">Quantidade de itens por página.</param>
    /// <param name="from">Data inicial do filtro.</param>
    /// <param name="to">Data final do filtro.</param>
    /// <param name="dispatcher">Disparador de queries.</param>
    /// <param name="logger">Logger da aplicação.</param>
    /// <param name="context">Contexto da requisição HTTP.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Uma lista paginada de agendamentos.</returns>
    private static async Task<IResult> GetMyBookingsAsync(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromServices] IQueryDispatcher dispatcher,
        [FromServices] ILogger<GetMyBookingsEndpoint> logger,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userIdClaim = context.User.FindFirst(AuthConstants.Claims.Subject)?.Value ?? 
                         context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var clientId))
        {
            return Error.Unauthorized("Autenticação necessária.").ToProblem();
        }

        if (from.HasValue && to.HasValue && from > to)
        {
            return Error.BadRequest("A data inicial ('from') não pode ser posterior à data final ('to').").ToProblem();
        }

        var normalizedPage = page ?? Pagination.DefaultPageNumber;
        if (normalizedPage < 1)
        {
            return Error.BadRequest($"O parâmetro 'page' deve ser maior ou igual a {Pagination.DefaultPageNumber}.").ToProblem();
        }

        var normalizedPageSize = pageSize ?? Pagination.DefaultPageSize;
        if (normalizedPageSize is < Pagination.MinPageSize or > Pagination.MaxPageSize)
        {
            return Error.BadRequest($"O parâmetro 'pageSize' deve estar entre {Pagination.MinPageSize} e {Pagination.MaxPageSize}.").ToProblem();
        }

        var correlationIdHeader = context.Request.Headers[AuthConstants.Headers.CorrelationId];
        var correlationIdRaw = correlationIdHeader.FirstOrDefault();
        var parsed = Guid.TryParse(correlationIdRaw, out var parsedId);
        var correlationId = parsed ? parsedId : Guid.NewGuid();

        if (!string.IsNullOrEmpty(correlationIdRaw) && !parsed)
        {
            logger.LogWarning("Failed to parse CorrelationId header '{HeaderKey}': raw value '{RawValue}'. Using new GUID instead.", 
                AuthConstants.Headers.CorrelationId, correlationIdRaw);
        }

        var query = new GetBookingsByClientQuery(clientId, correlationId, normalizedPage, normalizedPageSize, from?.UtcDateTime, to?.UtcDateTime);
        var result = await dispatcher.QueryAsync<GetBookingsByClientQuery, Result<PagedResult<ModuleBookingDto>>>(query, cancellationToken);

        return result.Match(
            onSuccess: bookings => Results.Ok(bookings),
            onFailure: error => error.ToProblem()
        );
    }
}
