using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Modules.Locations.Application.DTOs.Requests;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Locations.API.Endpoints;

/// <summary>
/// Endpoint para atualizar cidade permitida existente (Admin only)
/// </summary>
public class UpdateAllowedCityEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/api/v1/admin/allowed-cities/{id:guid}", UpdateAsync)
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
        var command = new UpdateAllowedCityCommand
        {
            Id = id,
            CityName = request.CityName,
            StateSigla = request.StateSigla,
            IbgeCode = request.IbgeCode,
            IsActive = request.IsActive
        };

        await commandDispatcher.SendAsync(command, cancellationToken);

        return Results.Ok(new Response<string>("Allowed city updated successfully"));
    }
}
