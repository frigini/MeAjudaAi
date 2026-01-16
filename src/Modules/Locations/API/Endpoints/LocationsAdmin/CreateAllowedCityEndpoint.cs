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
/// Endpoint para criar nova cidade permitida (Admin only)
/// </summary>
public class CreateAllowedCityEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost(string.Empty, CreateAsync)
            .WithName("CreateAllowedCity")
            .WithSummary("Criar nova cidade permitida")
            .WithDescription("Cria uma nova cidade permitida para operações de prestadores (apenas Admin)")
            .Produces<Response<Guid>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAdmin();

    private static async Task<IResult> CreateAsync(
        CreateAllowedCityRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand();

        var cityId = await commandDispatcher.SendAsync<CreateAllowedCityCommand, Guid>(command, cancellationToken);

        return Results.CreatedAtRoute("GetAllowedCityById", new { id = cityId }, new Response<Guid>(cityId, 201));
    }
}
