using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.Public.Me;

public class UpdateMyProviderProfileEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("api/v1/providers/me", UpdateMyProfileAsync)
            .WithName("UpdateMyProviderProfile")
            .WithTags("Providers - Me")
            .RequireAuthorization()
            .Produces<Response<ProviderDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> UpdateMyProfileAsync(
        HttpContext context,
        [FromBody] UpdateProviderProfileRequest request,
        IQueryDispatcher queryDispatcher,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return Results.BadRequest("O corpo da requisição é obrigatório");

        var userIdString = GetUserId(context);
        if (!Guid.TryParse(userIdString, out var userId))
            return Results.BadRequest("Formato de ID de usuário inválido");

        // Primeiro, obter o ID do prestador para o usuário atual
        var query = new GetProviderByUserIdQuery(userId);
        var providerResult = await queryDispatcher.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
            query, cancellationToken);

        if (providerResult.IsFailure || providerResult.Value is null)
        {
            return Results.NotFound("Perfil do provedor não encontrado para o usuário atual.");
        }

        var providerId = providerResult.Value.Id;

        // Despachar comando de atualização usando o ID do prestador encontrado
        var command = request.ToCommand(providerId);
        var result = await commandDispatcher.SendAsync<UpdateProviderProfileCommand, Result<ProviderDto>>(
            command, cancellationToken);
            
        return Handle(result);
    }
}
