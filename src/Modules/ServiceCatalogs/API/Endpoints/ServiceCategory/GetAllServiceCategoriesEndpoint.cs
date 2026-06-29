using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs.Requests.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.ServiceCategory;

/// <summary>
/// Endpoint para listar todas as categorias de serviços do catálogo.
/// Suporta filtro opcional por categorias ativas.
/// </summary>
public class GetAllServiceCategoriesEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Mapeia o endpoint GET / para listar todas as categorias.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet(ApiEndpoints.ServiceCatalogs.Categories.GetAll, GetAllAsync)
            .WithName(ApiEndpoints.ServiceCatalogs.Categories.Names.GetAll)
            .WithSummary("Listar todas as categorias")
            .WithDescription("""
                Retorna todas as categorias de serviços do catálogo.
                
                **Filtros Opcionais:**
                - `activeOnly` (bool): Filtra apenas categorias ativas (padrão: false)
                
                **Ordenação:**
                - Categorias são ordenadas por DisplayOrder (crescente)
                
                **Casos de Uso:**
                - Exibir menu de categorias para usuários
                - Administração do catálogo de categorias
                - Seleção de categoria ao criar serviço
                """)
            .Produces<Response<IReadOnlyList<ServiceCategoryDto>>>(StatusCodes.Status200OK)
            .RequirePermission(EPermission.ServiceCatalogsRead);

    /// <summary>
    /// Retorna todas as categorias de serviços, com opção de filtrar apenas ativas.
    /// </summary>
    private static async Task<IResult> GetAllAsync(
        [AsParameters] GetAllServiceCategoriesRequest request,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var qry = new GetAllServiceCategoriesQuery(request.ActiveOnly);
        var result = await queryDispatcher.QueryAsync<GetAllServiceCategoriesQuery, Result<IReadOnlyList<ServiceCategoryDto>>>(
            qry, cancellationToken);

        return Handle(result);
    }
}
