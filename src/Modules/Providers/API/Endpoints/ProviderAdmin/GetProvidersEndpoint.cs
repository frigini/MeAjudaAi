using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;

/// <summary>
/// Endpoint responsável pela listagem de prestadores de serviços com paginação e filtros opcionais.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para busca paginada de prestadores utilizando
/// arquitetura CQRS. Suporta filtros opcionais por nome, tipo e status de verificação.
/// </remarks>
public class GetProvidersEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de listagem de prestadores.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint GET em "/" com:
    /// - Parâmetros de paginação (page, pageSize)
    /// - Filtros opcionais (name, type, verificationStatus)
    /// - Documentação OpenAPI automática
    /// - Códigos de resposta apropriados
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/", GetProvidersAsync)
            .WithName("GetProviders")
            .WithSummary("Listar prestadores de serviços")
            .WithDescription("""
                Retorna uma lista paginada de prestadores de serviços com filtros opcionais.
                
                **Funcionalidades:**
                - 📄 Paginação configurável (padrão: página 1, 20 itens)
                - 🔍 Filtro por nome (busca parcial)
                - 🏷️ Filtro por tipo de prestador
                - ✅ Filtro por status de verificação
                - 🚫 Exclui prestadores deletados automaticamente
                
                **Parâmetros de consulta:**
                - `page`: Número da página (padrão: 1)
                - `pageSize`: Itens por página (padrão: 20, máximo: 100)
                - `name`: Filtro por nome (busca parcial, insensível a maiúsculas)
                - `type`: Filtro por tipo (0=Individual, 1=Company)
                - `verificationStatus`: Filtro por status (0=Pending, 1=Verified, 2=Rejected)
                """)
            .WithTags("Providers")
            .Produces<PagedResult<ProviderDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

    /// <summary>
    /// Processa a requisição de listagem de prestadores.
    /// </summary>
    /// <param name="queryDispatcher">Query dispatcher para processamento de queries</param>
    /// <param name="page">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Itens por página (padrão: 20)</param>
    /// <param name="name">Filtro opcional por nome</param>
    /// <param name="type">Filtro opcional por tipo</param>
    /// <param name="verificationStatus">Filtro opcional por status de verificação</param>
    /// <returns>Lista paginada de prestadores</returns>
    private static async Task<IResult> GetProvidersAsync(
        [FromServices] IQueryDispatcher queryDispatcher,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? name = null,
        [FromQuery] int? type = null,
        [FromQuery] int? verificationStatus = null)
    {
        // Validação básica de parâmetros
        if (page < 1)
            return TypedResults.Problem(detail: "Page must be greater than 0", statusCode: StatusCodes.Status400BadRequest);

        if (pageSize < 1 || pageSize > 100)
            return TypedResults.Problem(detail: "PageSize must be between 1 and 100", statusCode: StatusCodes.Status400BadRequest);

        // Cria query com filtros
        var query = new GetProvidersQuery(
            page, 
            pageSize, 
            name, 
            type, 
            verificationStatus);

        var result = await queryDispatcher.QueryAsync<GetProvidersQuery, Result<PagedResult<ProviderDto>>>(query);

        return Handle(result);
    }
}