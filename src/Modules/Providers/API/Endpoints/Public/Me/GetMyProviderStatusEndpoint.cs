using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.Public.Me;

/// <summary>
/// Endpoint para o prestador consultar seu status de aprovação e tier atual.
/// </summary>
/// <remarks>
/// Retorna um subconjunto do ProviderDto focado em status — evita expor dados sensíveis.
/// Usado pelo frontend para polling durante o onboarding (aguardando aprovação).
/// </remarks>
public class GetMyProviderStatusEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("me/status", GetMyStatusAsync)
            .WithName("GetMyProviderStatus")
            .WithTags("Providers - Me")
            .WithSummary("Status de aprovação do prestador")
            .WithDescription("Retorna o status atual de aprovação e tier do prestador autenticado.")
            .RequireAuthorization()
            .Produces<Response<ProviderStatusDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> GetMyStatusAsync(
        HttpContext context,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var userIdString = GetUserId(context);
        if (!Guid.TryParse(userIdString, out var userId))
            return BadRequest("Formato de ID de usuário inválido");

        var query = new GetProviderByUserIdQuery(userId);
        var result = await queryDispatcher.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
            query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error.Message);

        if (result.Value is null)
            return NotFound("Perfil do prestador não encontrado.");

        var statusDto = new ProviderStatusDto(
            Status: result.Value.Status,
            Tier: result.Value.Tier,
            VerificationStatus: result.Value.VerificationStatus,
            RejectionReason: result.Value.RejectionReason
        );

        return Results.Ok(new Response<ProviderStatusDto>(statusDto));
    }
}
