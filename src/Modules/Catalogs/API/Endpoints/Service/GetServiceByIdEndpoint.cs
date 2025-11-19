using MeAjudaAi.Modules.Catalogs.Application.DTOs;
using MeAjudaAi.Modules.Catalogs.Application.Queries.Service;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Catalogs.API.Endpoints.Service;

public class GetServiceByIdEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetServiceById")
            .WithSummary("Buscar serviço por ID")
            .Produces<Response<ServiceDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = new GetServiceByIdQuery(id);
        var result = await queryDispatcher.QueryAsync<GetServiceByIdQuery, Result<ServiceDto?>>(query, cancellationToken);

        if (result.IsSuccess && result.Value is null)
        {
            return Results.NotFound();
        }

        return Handle(result);
    }
}
