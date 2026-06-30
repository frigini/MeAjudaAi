using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;

/// <summary>
/// Endpoint para listar todos os serviços do catálogo.
/// Suporta filtro opcional por serviços ativos.
/// </summary>
public class GetAllServicesEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Mapeia o endpoint GET / para listar todos os serviços.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet(ApiEndpoints.ServiceCatalogs.Services.GetAll, GetAllAsync)
            .WithName(ApiEndpoints.ServiceCatalogs.Services.Names.GetAll)
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
            .Produces<Response<IReadOnlyList<ServiceListDto>>>(StatusCodes.Status200OK)
            .AllowAnonymous();

    /// <summary>
    /// Retorna todos os serviços, com opção de filtrar apenas ativos.
    /// </summary>
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
