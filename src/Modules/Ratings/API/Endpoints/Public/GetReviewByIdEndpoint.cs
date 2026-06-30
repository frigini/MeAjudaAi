using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Modules.Ratings.API.Endpoints.Public;

/// <summary>
/// Endpoint responsável pela consulta de uma avaliação (review) específica por ID.
/// </summary>
/// <remarks>
/// Endpoint público que permite consultar avaliações aprovadas pelo seu ID único.
/// Apenas avaliações com status aprovado são retornadas.
/// </remarks>
public class GetReviewByIdEndpoint : IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de consulta de avaliação por ID.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint GET em "/{id:guid}" com:
    /// - Acesso anônimo permitido
    /// - Validação automática de GUID para o parâmetro ID
    /// - Respostas estruturadas para sucesso (200) e não encontrado (404)
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Ratings.GetById, GetReviewByIdAsync)
            .WithName(ApiEndpoints.Ratings.Names.GetById)
            .WithSummary("Consultar avaliação por ID")
            .WithDescription("Recupera os dados de uma avaliação (review) aprovada pelo seu ID único.")
            .Produces<ProviderReviewResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .AllowAnonymous();
    }

    private static async Task<IResult> GetReviewByIdAsync(
        Guid id,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetReviewByIdQuery(id, Guid.NewGuid());
        var result = await dispatcher.QueryAsync<GetReviewByIdQuery, Result<ProviderReviewResponse>>(query, cancellationToken);

        return result.Match(
            onSuccess: review => Results.Ok(review),
            onFailure: error => error.ToProblem()
        );
    }
}
