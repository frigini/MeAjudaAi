using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Contracts.Contracts.Modules.Locations.DTOs;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MeAjudaAi.Shared.Authorization;

namespace MeAjudaAi.Modules.Locations.API.Endpoints.LocationsAdmin;

public class SearchLocationsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("search", SearchAsync)
            .WithName("SearchLocations")
            .WithSummary("Busca cidades/endereços para cadastro")
            .WithDescription("Retorna candidatos de localização baseados na query")
            .Produces<LocationCandidate[]>(StatusCodes.Status200OK)
            .RequirePermission(EPermission.LocationsManage);
    }

    private static async Task<IResult> SearchAsync(
        string query,
        IGeocodingService geocodingService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
        {
            return Results.Ok(Array.Empty<LocationCandidate>());
        }

        var results = await geocodingService.SearchAsync(query, cancellationToken);
        return Results.Ok(results);
    }
}
