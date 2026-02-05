using MeAjudaAi.Modules.ServiceCatalogs.Application.DTOs;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Service;
using MeAjudaAi.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using MeAjudaAi.Contracts.Models;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;

public class GetServiceByIdEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{id:guid}", GetByIdAsync)
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
