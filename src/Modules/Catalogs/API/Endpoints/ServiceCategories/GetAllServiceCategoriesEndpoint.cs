using MeAjudaAi.Modules.Catalogs.Application.Queries;
using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Catalogs.API.Endpoints.ServiceCategories;

public class GetAllServiceCategoriesEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/", GetAllAsync)
            .WithName("GetAllServiceCategories")
            .WithSummary("Listar todas as categorias")
            .Produces<Response<IReadOnlyList<ServiceCategoryDto>>>(StatusCodes.Status200OK);

    private static async Task<IResult> GetAllAsync(
        [AsParameters] GetAllCategoriesQuery query,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var qry = new GetAllServiceCategoriesQuery(query.ActiveOnly);
        var result = await queryDispatcher.QueryAsync<GetAllServiceCategoriesQuery, Result<IReadOnlyList<ServiceCategoryDto>>>(
            qry, cancellationToken);

        return Handle(result);
    }
}

public record GetAllCategoriesQuery(bool ActiveOnly = false);
