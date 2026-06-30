using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Modules.Locations.DTOs;
using MeAjudaAi.Modules.Locations.Application.Services;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Endpoints;

namespace MeAjudaAi.Modules.Locations.API.Endpoints.Admin;

public class SearchLocationsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("search", SearchAsync)
            .WithName(ApiEndpoints.Locations.Names.Search)
            .WithSummary("Busca cidades/endereços para cadastro")
            .WithDescription("Retorna candidatos de localização baseados na query")
            .Produces<LocationCandidate[]>(StatusCodes.Status200OK)
            .WithTags(LocationsEndpoints.Tag)
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
