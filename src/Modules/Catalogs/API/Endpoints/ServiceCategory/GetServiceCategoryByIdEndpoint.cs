using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Modules.Catalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Catalogs.API.Endpoints.ServiceCategory;

public class GetServiceCategoryByIdEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetServiceCategoryById")
            .WithSummary("Buscar categoria por ID")
            .Produces<Response<ServiceCategoryDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetServiceCategoryByIdQuery(id);
        var result = await queryDispatcher.QueryAsync<GetServiceCategoryByIdQuery, Result<ServiceCategoryDto?>>(
            query, cancellationToken);

        if (result.IsSuccess && result.Value == null)
            return Results.NotFound();

        return Handle(result);
    }
}
