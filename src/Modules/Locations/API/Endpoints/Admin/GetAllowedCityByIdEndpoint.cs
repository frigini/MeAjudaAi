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
/// Endpoint para buscar cidade permitida por ID (Admin only)
/// </summary>
public class GetAllowedCityByIdEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("{id:guid}", GetByIdAsync)
            .WithName(ApiEndpoints.Locations.Names.GetById)
            .WithSummary("Buscar cidade permitida por ID")
            .WithDescription("Recupera uma cidade permitida específica pelo seu ID")
            .Produces<Result<ModuleAllowedCityDto>>(StatusCodes.Status200OK)
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

        if (result is null)
            return Results.NotFound(Result<ModuleAllowedCityDto>.Failure("Cidade permitida não encontrada"));

        var contractResult = result.ToContract();

        return TypedResults.Ok(Result<ModuleAllowedCityDto>.Success(contractResult));
    }
}
