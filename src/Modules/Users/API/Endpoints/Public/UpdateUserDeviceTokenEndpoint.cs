using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Users.API.Endpoints.Public;

/// <summary>
/// Endpoint público responsável por atualizar o token do dispositivo do usuário para notificações push.
/// </summary>
[ExcludeFromCodeCoverage]
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
           .WithName(ApiEndpoints.Users.Names.UpdateDeviceToken)
           .WithSummary("Atualiza o token do dispositivo do usuário")
           .WithDescription("Registra ou atualiza o token do dispositivo para notificações push.");
    }

    /// <summary>
    /// Atualiza o token do dispositivo do usuário para notificações push.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <param name="dispatcher"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
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
