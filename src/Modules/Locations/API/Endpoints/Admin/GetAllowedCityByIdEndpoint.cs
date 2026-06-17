using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Locations.API.Endpoints.Admin;

/// <summary>
/// Endpoint para buscar cidade permitida por ID (Admin only)
/// </summary>
public class GetAllowedCityByIdEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("{id:guid}", GetByIdAsync)
            .WithName("GetAllowedCityById")
            .WithSummary("Buscar cidade permitida por ID")
            .WithDescription("Recupera uma cidade permitida específica pelo seu ID")
            .Produces<Response<AllowedCityDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(LocationsEndpoints.Tag)
            .RequireAdmin();

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetAllowedCityByIdQuery(id);

        var result = await queryDispatcher.QueryAsync<GetAllowedCityByIdQuery, AllowedCityDto?>(query, cancellationToken);

        return result is not null
            ? Results.Ok(new Response<AllowedCityDto>(result))
            : Results.NotFound(new Response<AllowedCityDto>(default, 404, "Cidade permitida não encontrada"));
    }
}
