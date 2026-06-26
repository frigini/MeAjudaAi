using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Modules.Ratings.API.Endpoints.Public;

/// <summary>
/// Endpoint responsável pela consulta de avaliações (reviews) de um prestador de serviço.
/// </summary>
/// <remarks>
/// Endpoint público que retorna todas as avaliações aprovadas de um prestador específico,
/// com suporte a paginação.
/// </remarks>
public class GetProviderReviewsEndpoint : IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de consultas de avaliações por prestador.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint GET em "/provider/{providerId:guid}" com:
    /// - Acesso anônimo permitido
    /// - Suporte a paginação via query string (page, pageSize)
    /// - Respostas estruturadas para sucesso (200)
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Ratings.GetByProvider, GetProviderReviewsAsync)
            .WithName("GetProviderReviews")
            .WithSummary("Consultar avaliações do prestador")
            .WithDescription("Recupera todas as avaliações aprovadas de um prestador de serviço específico, com paginação.")
            .Produces<IEnumerable<ProviderReviewResponse>>(StatusCodes.Status200OK)
            .AllowAnonymous();
    }

    private static async Task<IResult> GetProviderReviewsAsync(
        Guid providerId,
        [FromServices] IQueryDispatcher dispatcher,
        [FromQuery] int page = Pagination.DefaultPageNumber,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var (normalizedPage, normalizedPageSize) = NormalizePagination(page, pageSize);

        var query = new GetProviderReviewsQuery(providerId, normalizedPage, normalizedPageSize, Guid.NewGuid());
        var result = await dispatcher.QueryAsync<GetProviderReviewsQuery, Result<PagedResult<ProviderReviewResponse>>>(query, cancellationToken);

        return result.Match(
            onSuccess: reviews => Results.Ok(reviews),
            onFailure: error => error.ToProblem()
        );
    }

    private static (int Page, int PageSize) NormalizePagination(int page, int pageSize)
    {
        var normalizedPage = page < Pagination.DefaultPageNumber ? Pagination.DefaultPageNumber : page;
        var normalizedPageSize = pageSize;

        if (normalizedPageSize < Pagination.MinPageSize)
            normalizedPageSize = Pagination.MinPageSize;
        else if (normalizedPageSize > Pagination.MaxPageSize)
            normalizedPageSize = Pagination.MaxPageSize;

        return (normalizedPage, normalizedPageSize);
    }
}
