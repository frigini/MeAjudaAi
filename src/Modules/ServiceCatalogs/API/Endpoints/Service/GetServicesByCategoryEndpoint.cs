using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Mvc;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;

/// <summary>
/// Endpoint para listar serviços de uma categoria específica.
/// </summary>
public class GetServicesByCategoryEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Mapeia o endpoint GET /category/{categoryId} para listar serviços por categoria.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet(ApiEndpoints.ServiceCatalogs.Services.GetByCategory, GetByCategoryAsync)
            .WithName(ApiEndpoints.ServiceCatalogs.Services.Names.GetByCategory)
            .WithSummary("Listar serviços por categoria")
            .WithDescription("""
                Retorna todos os serviços de uma categoria específica.
                
                **Parâmetros:**
                - `categoryId` (route): ID da categoria
                - `activeOnly` (query, opcional): Filtrar apenas serviços ativos (padrão: false)
                
                **Casos de Uso:**
                - Exibir serviços disponíveis em uma categoria
                - Listar ofertas por categoria para provedores
                - Gestão de catálogo segmentado por categoria
                """)
            .Produces<Response<IReadOnlyList<ServiceListDto>>>(StatusCodes.Status200OK);

    /// <summary>
    /// Retorna todos os serviços da categoria informada.
    /// </summary>
    private static async Task<IResult> GetByCategoryAsync(
        Guid categoryId,
        [FromQuery] bool? activeOnly,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var queryWithCategory = new GetServicesByCategoryQuery(categoryId, activeOnly ?? false);
        var result = await queryDispatcher.QueryAsync<GetServicesByCategoryQuery, Result<IReadOnlyList<ServiceListDto>>>(queryWithCategory, cancellationToken);
        return Handle(result);
    }
}
