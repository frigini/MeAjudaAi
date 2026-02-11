using MeAjudaAi.Contracts;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.Public.Me;

public class GetMyProviderProfileEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("api/v1/providers/me", GetMyProfileAsync)
            .WithName("GetMyProviderProfile")
            .WithTags("Providers - Me")
            .RequireAuthorization()
            .Produces<Response<ProviderDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> GetMyProfileAsync(
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
        
        if (result.IsSuccess && result.Value is null)
        {
             return NotFound("Perfil do provedor não encontrado para o usuário atual.");
        }

        return Handle(result);
    }
}
