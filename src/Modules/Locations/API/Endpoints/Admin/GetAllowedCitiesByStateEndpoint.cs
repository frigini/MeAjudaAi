using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Locations.DTOs;
using MeAjudaAi.Modules.Locations.API.Mappers;
using MeAjudaAi.Modules.Locations.Application.DTOs;
using MeAjudaAi.Modules.Locations.Application.Queries;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.Locations.API.Endpoints.Admin;

/// <summary>
/// Endpoint para buscar cidades permitidas por estado (Admin only)
/// </summary>
public class GetAllowedCitiesByStateEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/state/{state}", GetByStateAsync)
            .WithName("GetAllowedCitiesByState")
            .WithSummary("Buscar cidades permitidas por estado")
            .WithDescription("Recupera todas as cidades permitidas de um estado específico (UF)")
            .Produces<Result<IReadOnlyList<ModuleAllowedCityDto>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags(LocationsEndpoints.Tag)
            .RequireAdmin();

    private static async Task<IResult> GetByStateAsync(
        string state,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(state) || state.Length != 2)
        {
            return Error.BadRequest("Sigla de estado inválida. Use 2 caracteres (ex: SP, MG, RJ).").ToProblem();
        }

        var query = new GetAllowedCitiesByStateQuery { State = state };

        var result = await queryDispatcher.QueryAsync<GetAllowedCitiesByStateQuery, IReadOnlyList<AllowedCityDto>>(
            query, cancellationToken);

        var contractResult = result.ToContract();

        return TypedResults.Ok(Result<IReadOnlyList<ModuleAllowedCityDto>>.Success(contractResult));
    }
}
