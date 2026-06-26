using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Contracts.Utilities.Constants;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
        [FromServices] IReviewQueries queries,
        [FromQuery] int page = Pagination.DefaultPageNumber,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = page < Pagination.DefaultPageNumber ? Pagination.DefaultPageNumber : page;
        var normalizedPageSize = pageSize < Pagination.MinPageSize ? Pagination.MinPageSize : (pageSize > Pagination.MaxPageSize ? Pagination.MaxPageSize : pageSize);

        var reviews = await queries.GetByProviderIdAsync(providerId, normalizedPage, normalizedPageSize, cancellationToken);

        var result = reviews.Select(r => new ProviderReviewResponse(
            r.Id.Value,
            r.Rating,
            r.Comment,
            r.CreatedAt));

        return Results.Ok(result);
    }
}
