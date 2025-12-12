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
/// Endpoint para listar todas as cidades permitidas (Admin only)
/// </summary>
public class GetAllAllowedCitiesEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/api/v1/admin/allowed-cities", GetAllAsync)
            .WithName("GetAllAllowedCities")
            .WithSummary("Get all allowed cities")
            .WithDescription("Retrieves all allowed cities (optionally only active ones)")
            .Produces<Response<IReadOnlyList<AllowedCityDto>>>(StatusCodes.Status200OK)
            .RequireAdmin();

    private static async Task<IResult> GetAllAsync(
        ICommandDispatcher commandDispatcher,
        bool onlyActive = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllAllowedCitiesQuery(onlyActive);

        var result = await commandDispatcher.DispatchAsync(query, cancellationToken);

        return result.Match(
            success => Results.Ok(Response.Success(success)),
            errors => HandleErrors(errors));
    }
}
