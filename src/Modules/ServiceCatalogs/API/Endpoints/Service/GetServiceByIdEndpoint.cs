using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;

/// <summary>
/// Endpoint para buscar um serviço pelo seu ID.
/// Retorna 404 se o serviço não for encontrado.
/// </summary>
public class GetServiceByIdEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Mapeia o endpoint GET /{id} para buscar serviço por ID.
    /// </summary>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet(ApiEndpoints.ServiceCatalogs.Services.GetById, GetByIdAsync)
            .WithName("GetServiceById")
            .WithSummary("Buscar serviço por ID")
            .WithDescription("""
                Retorna os detalhes completos de um serviço específico.
                
                **Retorno:**
                - Informações completas do serviço incluindo categoria
                - Status de ativação
                - Datas de criação e atualização
                
                **Casos de Uso:**
                - Exibir detalhes do serviço para edição
                - Visualizar informações completas do serviço
                - Validar existência do serviço
                """)
            .Produces<Response<ServiceDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    /// <summary>
    /// Retorna o serviço correspondente ao ID informado, ou 404 se não encontrado.
    /// </summary>
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
