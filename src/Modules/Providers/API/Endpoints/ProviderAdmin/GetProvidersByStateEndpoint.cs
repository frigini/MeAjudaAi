using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Extensions;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;

/// <summary>
/// Endpoint responsável pela consulta de prestadores por estado.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para busca de prestadores de serviços
/// filtrados por estado específico. Utiliza arquitetura CQRS e permite
/// consulta para descoberta de serviços em nível estadual.
/// </remarks>
public class GetProvidersByStateEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de consulta de prestadores por estado.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint GET em "/by-state/{state}" com:
    /// - Autorização por permissão (ProvidersRead)
    /// - Validação de parâmetro de estado
    /// - Documentação OpenAPI automática
    /// - Respostas estruturadas para lista de prestadores
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/by-state/{state}", GetProvidersByStateAsync)
            .WithName("GetProvidersByState")
            .WithSummary("Consultar prestadores por estado")
            .WithDescription("""
                Recupera lista de prestadores de serviços ativos em um estado específico.
                
                **Características:**
                - 🏛️ Busca por localização estadual
                - ⚡ Consulta otimizada para grandes volumes
                - 📋 Lista abrangente de prestadores
                - 🔍 Filtro automático por status ativo
                
                **Casos de uso:**
                - Descoberta de prestadores em estado específico
                - Análises regionais de prestadores
                - Listagem para cobertura estadual
                
                **Resposta incluirá:**
                - Lista de prestadores ativos no estado
                - Dados básicos de cada prestador
                - Informações de localização
                - Status de verificação
                """)
            .RequirePermission(EPermission.ProvidersRead)
            .Produces<Response<IReadOnlyList<ProviderDto>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError, typeof(ProblemDetails));

    /// <summary>
    /// Implementa a lógica de consulta de prestadores por estado.
    /// </summary>
    /// <param name="state">Nome do estado para busca</param>
    /// <param name="queryDispatcher">Dispatcher para envio de queries CQRS</param>
    /// <param name="logger">Logger para registro de erros</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Resultado HTTP com lista de prestadores ou erro apropriado</returns>
    /// <remarks>
    /// Processo da consulta:
    /// 1. Valida parâmetro de estado
    /// 2. Cria query usando mapper ToStateQuery
    /// 3. Envia query através do dispatcher CQRS
    /// 4. Retorna resposta HTTP com lista de prestadores
    /// </remarks>
    private static async Task<IResult> GetProvidersByStateAsync(
        string state,
        IQueryDispatcher queryDispatcher,
        ILogger<GetProvidersByStateEndpoint> logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = state.ToStateQuery();
            var result = await queryDispatcher.QueryAsync<GetProvidersByStateQuery, Result<IReadOnlyList<ProviderDto>>>(
                query, cancellationToken);

            return Handle(result);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error fetching providers by state: {State}", state);

            return Results.Problem(
                detail: "Ocorreu um erro interno ao buscar prestadores por estado. Consulte os logs.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Erro Interno do Servidor");
        }
    }
}
