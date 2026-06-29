using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Modules.Ratings.DTOs;
using MeAjudaAi.Modules.Ratings.Application.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Modules.Ratings.API.Endpoints.Public;

/// <summary>
/// Endpoint responsável pela criação de uma nova avaliação (review) de prestador de serviço.
/// </summary>
/// <remarks>
/// Endpoint autenticado que permite ao cliente criar uma avaliação para um prestador.
/// Valida se o cliente possui permissão antes de registrar a avaliação.
/// </remarks>
public class CreateReviewEndpoint : IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de criação de avaliação.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint POST em "/" com:
    /// - Autorização obrigatória (usuário autenticado)
    /// - Validação automática do corpo da requisição
    /// - Respostas estruturadas para sucesso (201), erro de validação (400) e não autorizado (401)
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Ratings.Create, CreateReviewAsync)
            .WithName(ApiEndpoints.Ratings.Names.Create)
            .WithSummary("Criar avaliação")
            .WithDescription("Registra uma nova avaliação (review) para um prestador de serviço.")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();
    }

    private static async Task<IResult> CreateReviewAsync(
        [FromBody] CreateReviewRequest request,
        [FromServices] ICommandHandler<CreateReviewCommand, Guid> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var customerId = ClaimHelpers.GetUserIdGuid(httpContext);

        if (customerId == null)
        {
            return Results.Unauthorized();
        }

        var command = new CreateReviewCommand(
            request.ProviderId,
            customerId.Value,
            request.Rating,
            request.Comment);

        var reviewId = await handler.HandleAsync(command, cancellationToken);

        return Results.Created($"/api/v1/{ApiEndpoints.Ratings.Base}/{reviewId}", reviewId);
    }
}
