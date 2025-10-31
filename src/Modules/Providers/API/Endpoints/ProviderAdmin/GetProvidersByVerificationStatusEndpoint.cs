using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;

/// <summary>
/// Endpoint responsável pela consulta de prestadores por status de verificação.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para busca de prestadores de serviços
/// filtrados por status de verificação. Utiliza arquitetura CQRS e permite
/// consulta administrativas para gerenciamento de prestadores.
/// </remarks>
public class GetProvidersByVerificationStatusEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de consulta de prestadores por status.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint GET em "/by-verification-status/{status}" com:
    /// - Autorização AdminOnly (apenas administradores)
    /// - Validação automática de enum para EVerificationStatus
    /// - Documentação OpenAPI automática
    /// - Respostas estruturadas para lista de prestadores
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/by-verification-status/{status}", GetProvidersByVerificationStatusAsync)
            .WithName("GetProvidersByVerificationStatus")
            .WithSummary("Consultar prestadores por status de verificação")
            .WithDescription("""
                Recupera lista de prestadores filtrados por status de verificação.
                
                **Status de verificação disponíveis:**
                - **Pending**: Aguardando verificação
                - **Verified**: Verificado e aprovado
                - **Rejected**: Rejeitado na verificação
                - **Suspended**: Suspenso temporariamente
                
                **Características:**
                - 🔒 Acesso restrito a administradores
                - ⚡ Consulta otimizada por índice de status
                - 📋 Lista para gestão administrativa
                - 🔍 Filtro preciso por status
                
                **Casos de uso:**
                - Gerenciamento de verificações pendentes
                - Auditoria de prestadores verificados
                - Controle de prestadores suspensos/rejeitados
                
                **Resposta incluirá:**
                - Lista de prestadores no status especificado
                - Dados completos para análise
                - Histórico de verificação quando aplicável
                - Informações de contato e documentos
                """)
            .RequireAuthorization("AdminOnly")
            .Produces<Response<IReadOnlyList<ProviderDto>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

    /// <summary>
    /// Implementa a lógica de consulta de prestadores por status de verificação.
    /// </summary>
    /// <param name="status">Status de verificação para filtro</param>
    /// <param name="queryDispatcher">Dispatcher para envio de queries CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Resultado HTTP com lista de prestadores ou erro apropriado</returns>
    /// <remarks>
    /// Processo da consulta:
    /// 1. Valida enum de status de verificação
    /// 2. Cria query usando mapper ToVerificationStatusQuery
    /// 3. Envia query através do dispatcher CQRS
    /// 4. Retorna resposta HTTP com lista de prestadores
    /// </remarks>
    private static async Task<IResult> GetProvidersByVerificationStatusAsync(
        EVerificationStatus status,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = status.ToVerificationStatusQuery();
        var result = await queryDispatcher.QueryAsync<GetProvidersByVerificationStatusQuery, Result<IReadOnlyList<ProviderDto>>>(
            query, cancellationToken);

        return Handle(result);
    }
}
