using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Locations.API.Endpoints;

/// <summary>
/// Endpoint para deletar cidade permitida (Admin only)
/// </summary>
public class DeleteAllowedCityEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/api/v1/admin/allowed-cities/{id:guid}", DeleteAsync)
            .WithName("DeleteAllowedCity")
            .WithSummary("Deletar cidade permitida")
            .WithDescription("Deleta uma cidade permitida")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAdmin();

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new DeleteAllowedCityCommand { Id = id };

        await commandDispatcher.SendAsync(command, cancellationToken);

        return Results.NoContent();
    }
}
