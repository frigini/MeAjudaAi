using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Locations.DTOs;
using MeAjudaAi.Modules.Locations.API.Mappers;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;

namespace MeAjudaAi.Modules.Locations.API.Endpoints.Admin;

/// <summary>
/// Endpoint para atualizar cidade permitida existente (Admin only)
/// </summary>
public class UpdateAllowedCityEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("{id:guid}", UpdateAsync)
            .WithName("UpdateAllowedCity")
            .WithSummary("Atualizar cidade permitida")
            .WithDescription("Atualiza uma cidade permitida existente")
            .Produces<Result<Unit>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags(LocationsEndpoints.Tag)
            .RequireAdmin();

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateAllowedCityRequestDto request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand(id);

        var result = await commandDispatcher.SendAsync<UpdateAllowedCityCommand, Result>(command, cancellationToken);
        
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return Results.Ok(Result<Unit>.Success(Unit.Value));
    }
}
