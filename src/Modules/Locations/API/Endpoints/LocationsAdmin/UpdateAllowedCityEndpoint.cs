using MeAjudaAi.Modules.Locations.API.Mappers;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.DTOs.Requests;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using MeAjudaAi.Shared.Models;
namespace MeAjudaAi.Modules.Locations.API.Endpoints.LocationsAdmin;

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
            .Produces<Response<string>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAdmin();

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateAllowedCityRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand(id);

        await commandDispatcher.SendAsync(command, cancellationToken);

        return Results.Ok(new Response<string>("Cidade permitida atualizada com sucesso"));
    }
}
