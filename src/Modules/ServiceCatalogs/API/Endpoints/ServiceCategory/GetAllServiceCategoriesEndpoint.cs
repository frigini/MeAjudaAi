using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using MeAjudaAi.Shared.Models;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.ServiceCategory;

public record GetAllCategoriesQuery(bool ActiveOnly = false);

public class GetAllServiceCategoriesEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/", GetAllAsync)
            .WithName("GetAllServiceCategories")
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
