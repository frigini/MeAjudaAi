using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.Public;

/// <summary>
/// Endpoint responsável por atualizar o token do dispositivo para notificações push do prestador.
/// </summary>
public class UpdateProviderDeviceTokenEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:guid}/device-token", UpdateDeviceTokenAsync)
           .RequireSelfOrAdmin()
           .Produces(StatusCodes.Status204NoContent)
           .ProducesProblem(StatusCodes.Status400BadRequest)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .WithTags("Providers")
           .WithName("UpdateProviderDeviceToken")
           .WithSummary("Atualiza o token do dispositivo do prestador")
           .WithDescription("Registra ou atualiza o token do dispositivo para notificações push do prestador.");
    }

    private static async Task<IResult> UpdateDeviceTokenAsync(
        [FromRoute] Guid id,
        [FromBody] ProviderDeviceTokenRequest request,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync<UpdateProviderDeviceTokenCommand, Result>(
            new UpdateProviderDeviceTokenCommand(id, request.DeviceToken ?? string.Empty), ct);

        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return result.Error.ToProblem();
    }
}

public sealed record ProviderDeviceTokenRequest(string DeviceToken);
