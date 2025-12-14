using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
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
            .WithDescription("Retrieves a specific allowed city by its ID")
            .Produces<Response<AllowedCityDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAdmin();

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetAllowedCityByIdQuery { Id = id };

        var result = await queryDispatcher.QueryAsync<GetAllowedCityByIdQuery, AllowedCityDto?>(query, cancellationToken);

        return result is not null
            ? Results.Ok(new Response<AllowedCityDto>(result))
            : Results.NotFound(new Response<AllowedCityDto>(default, 404, "Cidade permitida não encontrada"));
    }
}
