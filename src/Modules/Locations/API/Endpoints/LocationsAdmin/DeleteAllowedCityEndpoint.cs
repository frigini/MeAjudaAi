using MeAjudaAi.Modules.Locations.API.Mappers;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Locations.API.Endpoints.LocationsAdmin;

/// <summary>
/// Endpoint para deletar cidade permitida (Admin only)
/// </summary>
public class DeleteAllowedCityEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("{id:guid}", DeleteAsync)
            .WithName("DeleteAllowedCity")
            .WithSummary("Deletar cidade permitida")
            .WithDescription("Deleta uma cidade permitida")
            .Produces<Result>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAdmin();

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = id.ToDeleteCommand();

        await commandDispatcher.SendAsync(command, cancellationToken);

        // CORREÇÃO: Retornar 200 OK com Result.Success() para compatibilidade com ILocationsApi (Refit)
        // O frontend espera um objeto JSON { "isSuccess": true, ... }
        return Results.Ok(Result.Success());
    }
}
