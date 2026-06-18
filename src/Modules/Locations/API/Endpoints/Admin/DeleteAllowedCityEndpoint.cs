using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Locations.API.Mappers;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;

namespace MeAjudaAi.Modules.Locations.API.Endpoints.Admin;

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
            .WithTags(LocationsEndpoints.Tag)
            .RequireAdmin();

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = id.ToDeleteCommand();

        await commandDispatcher.SendAsync(command, cancellationToken);

        // CORREÇÃO: Retornar 200 OK com Result<Unit>.Success(Unit.Value) para compatibilidade com ILocationsApi (Refit)
        // O frontend espera um objeto JSON { "isSuccess": true, "value": {} }
        return Results.Ok(Result<Unit>.Success(Unit.Value));
    }
}
