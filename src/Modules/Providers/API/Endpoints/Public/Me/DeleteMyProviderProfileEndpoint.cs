using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.Public.Me;

/// <summary>
/// Endpoint para o próprio prestador excluir (logicamente) sua conta.
/// Requer autenticação.
/// </summary>
public class DeleteMyProviderProfileEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("me", DeleteMyProfileAsync)
            .WithName("DeleteMyProviderProfile")
            .WithTags("Providers - Me")
            .WithSummary("Excluir meu perfil de prestador")
            .WithDescription("Permite que o prestador logado exclua (logicamente) o próprio perfil.")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> DeleteMyProfileAsync(
        HttpContext context,
        IQueryDispatcher queryDispatcher,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var userIdString = GetUserId(context);
        if (!Guid.TryParse(userIdString, out var userId))
            return BadRequest("Formato de ID de usuário inválido");

        var query = new GetProviderByUserIdQuery(userId);
        var providerResult = await queryDispatcher.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
            query, cancellationToken);

        if (providerResult.IsFailure)
            return BadRequest(providerResult.Error);

        if (providerResult.Value is null)
            return NotFound("Perfil do provedor não encontrado para o usuário atual.");

        var command = new DeleteMyProviderProfileCommand(providerResult.Value.Id, userIdString);
        var result = await commandDispatcher.SendAsync<DeleteMyProviderProfileCommand, Result>(
            command, cancellationToken);

        return HandleNoContent(result);
    }
}
