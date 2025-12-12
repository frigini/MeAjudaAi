using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Locations.Infrastructure.API.Endpoints;

/// <summary>
/// Endpoint para deletar cidade permitida (Admin only)
/// </summary>
public class DeleteAllowedCityEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/api/v1/admin/allowed-cities/{id:guid}", DeleteAsync)
            .WithName("DeleteAllowedCity")
            .WithSummary("Delete allowed city")
            .WithDescription("Deletes an allowed city from the system")
            .Produces<Response<Unit>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAdmin();

    private static async Task<IResult> DeleteAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new DeleteAllowedCityCommand(id);

        var result = await commandDispatcher.DispatchAsync(command, cancellationToken);

        return result.Match(
            success => Results.Ok(Response.Success(success)),
            errors => HandleErrors(errors));
    }
}
