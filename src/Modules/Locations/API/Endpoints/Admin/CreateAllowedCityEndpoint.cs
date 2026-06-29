using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.Locations.DTOs;
using MeAjudaAi.Modules.Locations.API.Mappers;
using MeAjudaAi.Modules.Locations.Application.Commands;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Modules.Locations.API.Endpoints.Admin;

/// <summary>
/// Endpoint para criar nova cidade permitida (Admin only)
/// </summary>
public class CreateAllowedCityEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        =>         app.MapPost(string.Empty, CreateAsync)
            .WithName(ApiEndpoints.Locations.Names.Create)
            .WithSummary("Criar nova cidade permitida")
            .WithDescription("Cria uma nova cidade permitida para operações de prestadores (apenas Admin)")
            .WithTags(LocationsEndpoints.Tag)
            .Produces<Response<Guid>>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .RequireAdmin();

    private static async Task<IResult> CreateAsync(
        CreateAllowedCityRequestDto request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        var result = await commandDispatcher.SendAsync<CreateAllowedCityCommand, Result<Guid>>(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblem();
        }

        return Results.CreatedAtRoute("GetAllowedCityById", new { id = result.Value }, new Response<Guid>(result.Value, 201));
    }
}
