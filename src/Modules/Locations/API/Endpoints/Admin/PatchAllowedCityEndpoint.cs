using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Locations.DTOs;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Modules.Locations.API.Endpoints.Admin;

/// <summary>
/// Endpoint para atualizar parcialmente uma cidade permitida.
/// </summary>
public class PatchAllowedCityEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPatch("/{id:guid}", HandleAsync)
            .WithName("PatchAllowedCity")
            .WithSummary("Atualizar parcialmente cidade permitida")
            .WithDescription("Atualiza campos específicos de uma cidade permitida (Raio, Ativo)")
            .Produces<Result<Unit>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithTags(LocationsEndpoints.Tag)
            .RequireAdmin();

    private static async Task<IResult> HandleAsync(
        Guid id,
        PatchAllowedCityRequestDto request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new PatchAllowedCityCommand(id, request.ServiceRadiusKm, request.IsActive);
        var result = await commandDispatcher.SendAsync<PatchAllowedCityCommand, Result>(command, cancellationToken);
        
        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return TypedResults.Ok(Result<Unit>.Success(Unit.Value));
    }
}
