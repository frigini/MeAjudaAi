using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints.Public;

public class UpdateUserDeviceTokenEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:guid}/device-token", UpdateDeviceTokenAsync)
           .RequireSelfOrAdmin()
           .Produces(StatusCodes.Status204NoContent)
           .ProducesProblem(StatusCodes.Status400BadRequest)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .WithTags("Users")
           .WithName("UpdateUserDeviceToken")
           .WithSummary("Atualiza o token do dispositivo do usuário")
           .WithDescription("Registra ou atualiza o token do dispositivo para notificações push.");
    }

    private static async Task<IResult> UpdateDeviceTokenAsync(
        [FromRoute] Guid id,
        [FromBody] DeviceTokenRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync<UpdateUserDeviceTokenCommand, Result>(new UpdateUserDeviceTokenCommand(id, request.DeviceToken, Guid.NewGuid()), ct);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return result.Error.ToProblem();
    }
}


public sealed record DeviceTokenRequest(string? DeviceToken);
