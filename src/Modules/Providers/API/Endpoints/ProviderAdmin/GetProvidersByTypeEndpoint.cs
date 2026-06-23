using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;

/// <summary>
/// Endpoint responsável pela consulta de prestadores por tipo.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para busca de prestadores de serviços
/// filtrados por tipo (Individual ou Company). Utiliza arquitetura CQRS e
/// permite consulta categorizada por estrutura organizacional.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public class GetProvidersByTypeEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de consulta de prestadores por tipo.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint GET em "/by-type/{type}" com:
    /// - Autorização por permissão (ProvidersRead)
    /// - Validação automática de enum para EProviderType
    /// - Documentação OpenAPI automática
    /// - Respostas estruturadas para lista de prestadores
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/by-type/{type}", GetProvidersByTypeAsync)
            .WithName("GetProvidersByType")
            .WithSummary("Consultar prestadores por tipo")
            .WithDescription("""
                Recupera lista de prestadores de serviços filtrados por tipo organizacional.
                
                **Tipos disponíveis:**
                - **Individual** (1): Prestadores pessoa física
                - **Company** (2): Prestadores pessoa jurídica
                
                **Características:**
                - 🏢 Busca por estrutura organizacional
                - ⚡ Consulta otimizada por índice de tipo
                - 📋 Lista categorizada de prestadores
                - 🔍 Filtro automático por status ativo
                
                **Casos de uso:**
                - Descoberta de prestadores individuais vs empresas
                - Análises por tipo de prestador
                - Segmentação de mercado
                
                **Resposta incluirá:**
                - Lista de prestadores do tipo especificado
                - Dados básicos adequados ao tipo
                - Informações organizacionais relevantes
                - Status de verificação
                """)
            .RequirePermission(EPermission.ProvidersRead)
            .Produces<Response<IReadOnlyList<ProviderDto>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

    /// <summary>
    /// Implementa a lógica de consulta de prestadores por tipo.
    /// </summary>
    /// <param name="type">Tipo do prestador (Individual ou Company)</param>
    /// <param name="queryDispatcher">Dispatcher para envio de queries CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Resultado HTTP com lista de prestadores ou erro apropriado</returns>
    /// <remarks>
    /// Processo da consulta:
    /// 1. Valida enum de tipo do prestador
    /// 2. Cria query usando mapper ToTypeQuery
    /// 3. Envia query através do dispatcher CQRS
    /// 4. Retorna resposta HTTP com lista de prestadores
    /// </remarks>
    private static async Task<IResult> GetProvidersByTypeAsync(
        EProviderType type,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = type.ToTypeQuery();
        var result = await queryDispatcher.QueryAsync<GetProvidersByTypeQuery, Result<IReadOnlyList<ProviderDto>>>(
            query, cancellationToken);

        return Handle(result);
    }
}
