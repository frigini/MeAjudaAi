using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.ServiceCategory;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.ServiceCategory;

/// <summary>
/// Endpoint para buscar uma categoria de serviço pelo seu ID.
/// Retorna 404 se a categoria não for encontrada.
/// </summary>
public class GetServiceCategoryByIdEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Mapeia o endpoint GET /{id} para buscar categoria por ID.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet(ApiEndpoints.ServiceCatalogs.Categories.GetById, GetByIdAsync)
            .WithName(ApiEndpoints.ServiceCatalogs.Categories.Names.GetById)
            .WithSummary("Buscar categoria por ID")
            .WithDescription("""
                Retorna os detalhes completos de uma categoria específica.
                
                **Retorno:**
                - Informações completas da categoria
                - Status de ativação
                - DisplayOrder para ordenação
                - Datas de criação e atualização
                
                **Casos de Uso:**
                - Exibir detalhes da categoria para edição
                - Validar existência de categoria
                - Visualizar informações completas
                """)
            .Produces<Response<ServiceCategoryDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    /// <summary>
    /// Retorna a categoria correspondente ao ID informado, ou 404 se não encontrada.
    /// </summary>
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
