using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Locations.API.Endpoints.LocationsAdmin;

/// <summary>
/// Endpoint para listar todas as cidades permitidas (Admin only)
/// </summary>
public class GetAllAllowedCitiesEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet(string.Empty, GetAllAsync)
            .WithName("GetAllAllowedCities")
            .WithSummary("Listar todas as cidades permitidas")
            .WithDescription("Recupera todas as cidades permitidas (opcionalmente apenas as ativas)")
            .Produces<Response<IReadOnlyList<AllowedCityDto>>>(StatusCodes.Status200OK)
            .RequireAdmin();

    private static async Task<IResult> GetAllAsync(
        bool onlyActive = false,
        IQueryDispatcher queryDispatcher = default!,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllAllowedCitiesQuery { OnlyActive = onlyActive };

        var result = await queryDispatcher.QueryAsync<GetAllAllowedCitiesQuery, IReadOnlyList<AllowedCityDto>>(query, cancellationToken);

        return Results.Ok(new Response<IReadOnlyList<AllowedCityDto>>(result));
    }
}
