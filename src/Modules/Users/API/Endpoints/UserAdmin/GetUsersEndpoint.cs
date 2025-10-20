using MeAjudaAi.Modules.Users.API.Mappers;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Modules.Users.Application.Queries;
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
using Microsoft.OpenApi.Models;

namespace MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;

/// <summary>
/// Endpoint responsável pela consulta paginada de usuários do sistema.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para listagem paginada de usuários
/// utilizando arquitetura CQRS. Suporta filtros e parâmetros de paginação
/// para otimizar performance em grandes volumes de dados. Requer autorização
/// apropriada para acesso aos dados dos usuários.
/// </remarks>
public class GetUsersEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de consulta de usuários.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint GET em "/" com:
    /// - Autorização SelfOrAdmin (usuário pode ver próprios dados ou admin vê todos)
    /// - Suporte a parâmetros de paginação via query string
    /// - Documentação OpenAPI automática
    /// - Resposta paginada estruturada
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet(ApiEndpoints.Users.GetAll, GetUsersAsync)
            .WithName("GetUsers")
            .WithSummary("Consultar usuários paginados")
            .WithDescription("""
                Recupera uma lista paginada de usuários do sistema com suporte a filtros de busca.
                
                **Características:**
                - 🔍 Busca por email, nome de usuário, nome ou sobrenome
                - 📄 Paginação otimizada com metadados
                - ⚡ Cache automático para consultas frequentes
                - 🔒 Controle de acesso baseado em papéis
                
                **Parâmetros de busca:**
                - `searchTerm`: Termo para filtrar usuários (busca em email, username, nome)
                - `pageNumber`: Número da página (padrão: 1)
                - `pageSize`: Tamanho da página (padrão: 10, máximo: 100)
                
                **Exemplos de uso:**
                - Buscar usuários: `?searchTerm=joão`
                - Paginação: `?pageNumber=2&pageSize=20`
                - Combinado: `?searchTerm=admin&pageNumber=1&pageSize=10`
                """)
            .WithTags("Usuários - Administração")
            .Produces<PagedResponse<IEnumerable<UserDto>>>(StatusCodes.Status200OK, "application/json")
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces<AuthenticationErrorResponse>(StatusCodes.Status401Unauthorized, "application/json")
            .Produces<AuthorizationErrorResponse>(StatusCodes.Status403Forbidden, "application/json")
            .Produces<RateLimitErrorResponse>(StatusCodes.Status429TooManyRequests, "application/json")
            .Produces<InternalServerErrorResponse>(StatusCodes.Status500InternalServerError, "application/json")
            .RequirePermission(Permission.UsersList)
            .WithOpenApi(operation =>
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "searchTerm",
                    In = ParameterLocation.Query,
                    Description = "Termo de busca para filtrar por email, username, nome ou sobrenome",
                    Required = false,
                    Schema = new OpenApiSchema { Type = "string", Example = new Microsoft.OpenApi.Any.OpenApiString("joão") }
                });

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "pageNumber",
                    In = ParameterLocation.Query,
                    Description = "Número da página (base 1)",
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "integer",
                        Minimum = 1,
                        Default = new Microsoft.OpenApi.Any.OpenApiInteger(1),
                        Example = new Microsoft.OpenApi.Any.OpenApiInteger(1)
                    }
                });

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "pageSize",
                    In = ParameterLocation.Query,
                    Description = "Quantidade de itens por página",
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "integer",
                        Minimum = 1,
                        Maximum = 100,
                        Default = new Microsoft.OpenApi.Any.OpenApiInteger(10),
                        Example = new Microsoft.OpenApi.Any.OpenApiInteger(10)
                    }
                });

                return operation;
            });

    /// <summary>
    /// Processa requisição de consulta de usuários de forma assíncrona.
    /// </summary>
    /// <param name="pageNumber">Número da página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 10)</param>
    /// <param name="searchTerm">Termo de busca (opcional)</param>
    /// <param name="queryDispatcher">Dispatcher para envio de queries CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado HTTP contendo:
    /// - 200 OK: Lista paginada de usuários com metadados de paginação
    /// - 400 Bad Request: Erro de validação nos parâmetros
    /// </returns>
    /// <remarks>
    /// Fluxo de execução:
    /// 1. Extrai parâmetros de paginação da query string
    /// 2. Cria query CQRS com parâmetros validados
    /// 3. Envia query através do dispatcher
    /// 4. Retorna resposta paginada estruturada com metadados
    /// 
    /// Suporta parâmetros: PageNumber, PageSize, SearchTerm
    /// </remarks>
    private static async Task<IResult> GetUsersAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        IQueryDispatcher queryDispatcher = null!,
        CancellationToken cancellationToken = default)
    {
        var request = new GetUsersRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var query = request.ToUsersQuery();
        var result = await queryDispatcher.QueryAsync<GetUsersQuery, Result<PagedResult<UserDto>>>(
            query, cancellationToken);

        return HandlePagedResult(result);
    }
}
