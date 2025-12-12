using MeAjudaAi.Modules.Locations.Application.Queries;
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
/// Endpoint para buscar cidade permitida por ID (Admin only)
/// </summary>
public class GetAllowedCityByIdEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/api/v1/admin/allowed-cities/{id:guid}", GetByIdAsync)
            .WithName("GetAllowedCityById")
            .WithSummary("Get allowed city by ID")
            .WithDescription("Retrieves an allowed city by its unique identifier")
            .Produces<Response<AllowedCityDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAdmin();

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetAllowedCityByIdQuery(id);

        var result = await commandDispatcher.DispatchAsync(query, cancellationToken);

        return result.Match(
            success => success is null
                ? Results.NotFound(Response.Error($"Cidade permitida com ID '{id}' não encontrada"))
                : Results.Ok(Response.Success(success)),
            errors => HandleErrors(errors));
    }
}
