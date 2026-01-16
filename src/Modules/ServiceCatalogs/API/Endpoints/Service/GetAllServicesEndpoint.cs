using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using MeAjudaAi.Shared.Models;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;

public class GetAllServicesEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/", GetAllAsync)
            .WithName("GetAllServices")
            .WithSummary("Listar todos os serviços")
            .WithDescription("""
                Retorna todos os serviços do catálogo.
                
                **Filtros Opcionais:**
                - `activeOnly` (bool): Filtra apenas serviços ativos (padrão: false)
                
                **Casos de Uso:**
                - Listar todo o catálogo de serviços
                - Obter apenas serviços ativos para exibição pública
                - Administração do catálogo completo
                """)
            .Produces<Response<IReadOnlyList<ServiceListDto>>>(StatusCodes.Status200OK);

    private static async Task<IResult> GetAllAsync(
        [AsParameters] GetAllServicesQuery query,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var result = await queryDispatcher.QueryAsync<GetAllServicesQuery, Result<IReadOnlyList<ServiceListDto>>>(
            query, cancellationToken);
        return Handle(result);
    }
}
