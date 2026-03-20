using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.Public.Me;

public class DeactivateMyProviderProfileEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("me/deactivate", DeactivateMyProfileAsync)
            .WithName("DeactivateMyProviderProfile")
            .WithTags("Providers - Me")
            .RequireAuthorization()
            .Produces<Response<object>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> DeactivateMyProfileAsync(
        HttpContext context,
        IQueryDispatcher queryDispatcher,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var userIdString = GetUserId(context);
        if (!Guid.TryParse(userIdString, out var userId))
            return BadRequest("Formato de ID de usuário inválido");

        // Obter o ID do prestador para o usuário atual
        var query = new GetProviderByUserIdQuery(userId);
        var providerResult = await queryDispatcher.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
            query, cancellationToken);

        if (providerResult.IsFailure)
        {
            return BadRequest(providerResult.Error.Message);
        }

        if (providerResult.Value is null)
        {
            return NotFound("Perfil do provedor não encontrado para o usuário atual.");
        }

        var providerId = providerResult.Value.Id;

        // Despachar comando de desativação de perfil usando o ID do prestador encontrado
        var command = new DeactivateProviderProfileCommand(providerId, userIdString);
        var result = await commandDispatcher.SendAsync<DeactivateProviderProfileCommand, Result>(
            command, cancellationToken);
            
        return Handle(result);
    }
}
