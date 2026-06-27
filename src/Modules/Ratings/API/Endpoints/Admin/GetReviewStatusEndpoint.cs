using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Modules.Ratings.API.Endpoints.Admin;

/// <summary>
/// Endpoint responsável pela consulta do status de uma avaliação (review) específica.
/// </summary>
/// <remarks>
/// Endpoint administrador que permite consultar o status atual de uma avaliação
/// utilizando arquitetura CQRS. Mapeia os valores do domínio para os valores
/// do contrato.
/// </remarks>
public class GetReviewStatusEndpoint : IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de consulta de status de avaliação.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint GET em "/{id:guid}/status" com:
    /// - Autorização AdminOnly (apenas administradores)
    /// - Validação automática de GUID para o parâmetro ID
    /// - Respostas estruturadas para sucesso (200), não encontrado (404), não autorizado (401) e proibido (403)
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Ratings.GetStatus, GetReviewStatusAsync)
            .WithName("GetReviewStatus")
            .WithSummary("Consultar status de avaliação")
            .WithDescription("Recupera o status atual de uma avaliação (review) pelo seu ID.")
            .Produces<ReviewStatusResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization("AdminOnly");
    }

    private static async Task<IResult> GetReviewStatusAsync(
        Guid id,
        [FromServices] IQueryDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetReviewStatusQuery(id, Guid.NewGuid());
        var result = await dispatcher.QueryAsync<GetReviewStatusQuery, Result<ReviewStatusResponse>>(query, cancellationToken);

        return result.Match(
            onSuccess: status => Results.Ok(status),
            onFailure: error => error.ToProblem()
        );
    }
}
