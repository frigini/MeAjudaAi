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
using Microsoft.AspNetCore.Mvc;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;

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
            return Results.Problem(
                detail: result.Error.Message,
                statusCode: result.Error.StatusCode,
                title: "Erro ao criar cidade permitida");
        }

        return Results.CreatedAtRoute("GetAllowedCityById", new { id = result.Value }, new Response<Guid>(result.Value, 201));
    }
}
