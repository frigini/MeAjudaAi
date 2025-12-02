using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Models;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;

/// <summary>
/// Endpoint responsável pela consulta paginada de prestadores de serviços do sistema.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para listagem paginada de prestadores
/// utilizando arquitetura CQRS. Suporta filtros e parâmetros de paginação
/// para otimizar performance em grandes volumes de dados. Requer autorização
/// apropriada para acesso aos dados dos prestadores.
/// </remarks>
public class GetProvidersEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de consulta de prestadores.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint GET em "/" com:
    /// - Autorização baseada em permissões (usuário pode ver prestadores)
    /// - Suporte a parâmetros de paginação via query string
    /// - Documentação OpenAPI automática
    /// - Resposta paginada estruturada
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet(ApiEndpoints.Providers.GetAll, GetProvidersAsync)
            .WithName("GetProviders")
            .WithSummary("Consultar prestadores paginados")
            .WithDescription("""
                Recupera uma lista paginada de prestadores de serviços do sistema com suporte a filtros de busca.
                
                **Características:**
                - 🔍 Busca por nome, tipo de serviço e status de verificação
                - 📄 Paginação otimizada com metadados
                - ⚡ Cache automático para consultas frequentes
                - 🔒 Controle de acesso baseado em papéis
                - 🌍 Restrição geográfica (piloto em cidades específicas)
                
                **Restrição geográfica (HTTP 451):**
                
                Este endpoint está sujeito a restrições geográficas durante a fase piloto.
                O acesso é permitido apenas para usuários nas seguintes cidades:
                
                - **Muriaé** (MG) - IBGE: 3143906
                - **Itaperuna** (RJ) - IBGE: 3302205
                - **Linhares** (ES) - IBGE: 3203205
                
                A localização é determinada através dos headers HTTP:
                - `X-User-City`: Nome da cidade
                - `X-User-State`: Sigla do estado (UF)
                - `X-User-Location`: Combinação "cidade|estado"
                
                Se o acesso for bloqueado, você receberá HTTP 451 com detalhes:
                - Sua localização detectada
                - Lista de cidades permitidas
                - Códigos IBGE para validação
                
                **Parâmetros de busca:**
                - `name`: Termo para filtrar prestadores por nome
                - `type`: Filtro por tipo de serviço (ID numérico)
                - `verificationStatus`: Status de verificação (ID numérico)
                - `pageNumber`: Número da página (padrão: 1)
                - `pageSize`: Tamanho da página (padrão: 10, máximo: 100)
                
                **Exemplos de uso:**
                - Buscar prestadores: `?name=joão`
                - Por tipo: `?type=1`
                - Por status: `?verificationStatus=2`
                - Paginação: `?pageNumber=2&pageSize=20`
                - Combinado: `?name=médico&type=1&pageNumber=1&pageSize=10`
                """)
            .WithTags("Prestadores - Administração")
            .Produces<PagedResponse<IEnumerable<ProviderDto>>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces<AuthenticationErrorResponse>(StatusCodes.Status401Unauthorized, "application/json")
            .Produces<AuthorizationErrorResponse>(StatusCodes.Status403Forbidden, "application/json")
            .Produces<GeographicRestrictionErrorResponse>(451, "application/json") // HTTP 451 - Unavailable For Legal Reasons (RFC 7725)
            .Produces<RateLimitErrorResponse>(StatusCodes.Status429TooManyRequests, "application/json")
            .Produces<InternalServerErrorResponse>(StatusCodes.Status500InternalServerError, "application/json")
            .RequirePermission(Permission.ProvidersList);

    /// <summary>
    /// Processa requisição de consulta de prestadores de forma assíncrona.
    /// </summary>
    /// <param name="pageNumber">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 10)</param>
    /// <param name="name">Filtro por nome do prestador (opcional)</param>
    /// <param name="type">Filtro por tipo de serviço (opcional)</param>
    /// <param name="verificationStatus">Filtro por status de verificação (opcional)</param>
    /// <param name="queryDispatcher">Dispatcher para envio de queries CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado HTTP contendo:
    /// - 200 OK: Lista paginada de prestadores com metadados de paginação
    /// - 400 Bad Request: Erro de validação nos parâmetros
    /// </returns>
    /// <remarks>
    /// Fluxo de execução:
    /// 1. Extrai parâmetros de paginação da query string
    /// 2. Cria query CQRS com filtros validados
    /// 3. Envia query através do dispatcher
    /// 4. Retorna resposta paginada estruturada com metadados
    /// 
    /// Suporta parâmetros: PageNumber, PageSize, Name, Type, VerificationStatus
    /// </remarks>
    private static async Task<IResult> GetProvidersAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? name = null,
        int? type = null,
        int? verificationStatus = null,
        IQueryDispatcher queryDispatcher = null!,
        CancellationToken cancellationToken = default)
    {
        var request = new GetProvidersRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Name = name,
            Type = type,
            VerificationStatus = verificationStatus
        };

        var query = request.ToProvidersQuery();
        var result = await queryDispatcher.QueryAsync<GetProvidersQuery, Result<PagedResult<ProviderDto>>>(
            query, cancellationToken);

        return HandlePagedResult(result);
    }
}
