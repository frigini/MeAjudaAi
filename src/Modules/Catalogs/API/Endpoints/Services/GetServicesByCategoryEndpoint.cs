using MeAjudaAi.Modules.Catalogs.Application.Queries;
using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Catalogs.API.Endpoints.Services;

public class GetServicesByCategoryEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/category/{categoryId:guid}", GetByCategoryAsync)
            .WithName("GetServicesByCategory")
            .WithSummary("Listar serviços por categoria")
            .Produces<Response<IReadOnlyList<ServiceListDto>>>(StatusCodes.Status200OK);

    private static async Task<IResult> GetByCategoryAsync(
        Guid categoryId,
        bool activeOnly,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetServicesByCategoryQuery(categoryId, activeOnly);
        var result = await queryDispatcher.QueryAsync<GetServicesByCategoryQuery, Result<IReadOnlyList<ServiceListDto>>>(
            query, cancellationToken);
        return Handle(result);
    }
}
