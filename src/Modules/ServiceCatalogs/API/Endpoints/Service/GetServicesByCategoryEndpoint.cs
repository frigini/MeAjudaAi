using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;

public class GetServicesByCategoryEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/category/{categoryId:guid}", GetByCategoryAsync)
            .WithName("GetServicesByCategory")
            .WithSummary("Listar serviços por categoria")
            .Produces<Response<IReadOnlyList<ServiceListDto>>>(StatusCodes.Status200OK);

    private static async Task<IResult> GetByCategoryAsync(
        Guid categoryId,
        [AsParameters] GetServicesByCategoryQuery query,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var queryWithCategory = query with { CategoryId = categoryId };
        var result = await queryDispatcher.QueryAsync<GetServicesByCategoryQuery, Result<IReadOnlyList<ServiceListDto>>>(queryWithCategory, cancellationToken);
        return Handle(result);
    }
}
