using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Modules.Ratings.Application.Queries;
using MeAjudaAi.Modules.Ratings.Domain.ValueObjects;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Mvc;
using ContractsEnumEReviewStatus = MeAjudaAi.Contracts.Modules.Ratings.Enums.EReviewStatus;
using DomainEnumEReviewStatus = MeAjudaAi.Modules.Ratings.Domain.Enums.EReviewStatus;

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
    /// - Autorização AdminPolicy (apenas administradores)
    /// - Validação automática de GUID para o parâmetro ID
    /// - Respostas estruturadas para sucesso (200) e não encontrado (404)
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Ratings.GetStatus, GetReviewStatusAsync)
            .WithName("GetReviewStatus")
            .WithSummary("Consultar status de avaliação")
            .WithDescription("Recupera o status atual de uma avaliação (review) pelo seu ID.")
            .Produces<ReviewStatusResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminPolicy");
    }

    private static async Task<IResult> GetReviewStatusAsync(
        Guid id,
        [FromServices] IReviewQueries queries,
        CancellationToken cancellationToken)
    {
        var review = await queries.GetByIdAsync((ReviewId)id, cancellationToken);

        if (review == null)
            return Results.NotFound();

        return Results.Ok(new ReviewStatusResponse(
            review.Id.Value,
            MapReviewStatus(review.Status)));
    }

    private static ContractsEnumEReviewStatus MapReviewStatus(DomainEnumEReviewStatus status) => status switch
    {
        DomainEnumEReviewStatus.Pending => ContractsEnumEReviewStatus.Pending,
        DomainEnumEReviewStatus.Approved => ContractsEnumEReviewStatus.Approved,
        DomainEnumEReviewStatus.Rejected => ContractsEnumEReviewStatus.Rejected,
        DomainEnumEReviewStatus.Flagged => ContractsEnumEReviewStatus.Flagged,
        _ => throw new NotSupportedException($"Status {status} não é suportado")
    };
}
