using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Locations.DTOs;
using MeAjudaAi.Modules.Locations.API.Mappers;
using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Locations.API.Endpoints.Admin;

/// <summary>
/// Endpoint para listar todas as cidades permitidas (Admin only)
/// </summary>
public class GetAllAllowedCitiesEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet(string.Empty, GetAllAsync)
            .WithName(ApiEndpoints.Locations.Names.GetAll)
            .WithSummary("Listar todas as cidades permitidas")
            .WithDescription("Recupera todas as cidades permitidas (opcionalmente apenas as ativas)")
            .Produces<Result<IReadOnlyList<ModuleAllowedCityDto>>>(StatusCodes.Status200OK)
            .WithTags(LocationsEndpoints.Tag)
            .RequireAdmin();

    private static async Task<IResult> GetAllAsync(
        bool onlyActive = false,
        IQueryDispatcher queryDispatcher = default!,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllAllowedCitiesQuery { OnlyActive = onlyActive };

        var result = await queryDispatcher.QueryAsync<GetAllAllowedCitiesQuery, IReadOnlyList<AllowedCityDto>>(query, cancellationToken);
        
        // Map to contract DTOs
        var contractResult = result.ToContract();

        return TypedResults.Ok(Result<IReadOnlyList<ModuleAllowedCityDto>>.Success(contractResult));
    }
}
