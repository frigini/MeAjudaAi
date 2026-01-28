using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;

/// <summary>
/// Endpoint responsável pela consulta de prestadores por cidade.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para busca de prestadores de serviços
/// filtrados por cidade específica. Utiliza arquitetura CQRS e permite
/// consulta pública para facilitar descoberta de serviços.
/// </remarks>
public class GetProvidersByCityEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de consulta de prestadores por cidade.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint GET em "/by-city/{city}" com:
    /// - Autorização obrigatória (RequireAuthorization)
    /// - Validação de parâmetro de cidade
    /// - Documentação OpenAPI automática
    /// - Respostas estruturadas para lista de prestadores
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/by-city/{city}", GetProvidersByCityAsync)
            .WithName("GetProvidersByCity")
            .WithSummary("Consultar prestadores por cidade")
            .WithDescription("""
                Recupera lista de prestadores de serviços ativos em uma cidade específica.
                
                **Características:**
                - 🏙️ Busca por localização geográfica
                - ⚡ Consulta otimizada com índices
                - 📋 Lista completa de prestadores disponíveis
                - 🔍 Filtro automático por status ativo
                
                **Casos de uso:**
                - Descoberta de prestadores em cidade específica
                - Listagem para usuários finais
                - Integração com sistemas de busca
                
                **Resposta incluirá:**
                - Lista de prestadores ativos na cidade
                - Dados básicos de cada prestador
                - Informações de contato
                - Status de verificação
                """)
            .RequireAuthorization()
            .Produces<Response<IReadOnlyList<ProviderDto>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

    /// <summary>
    /// Implementa a lógica de consulta de prestadores por cidade.
    /// </summary>
    /// <param name="city">Nome da cidade para busca</param>
    /// <param name="queryDispatcher">Dispatcher para envio de queries CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Resultado HTTP com lista de prestadores ou erro apropriado</returns>
    /// <remarks>
    /// Processo da consulta:
    /// 1. Valida parâmetro de cidade
    /// 2. Cria query usando mapper ToCityQuery
    /// 3. Envia query através do dispatcher CQRS
    /// 4. Retorna resposta HTTP com lista de prestadores
    /// </remarks>
    private static async Task<IResult> GetProvidersByCityAsync(
        string city,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = city.ToCityQuery();
        var result = await queryDispatcher.QueryAsync<GetProvidersByCityQuery, Result<IReadOnlyList<ProviderDto>>>(
            query, cancellationToken);

        return Handle(result);
    }
}
