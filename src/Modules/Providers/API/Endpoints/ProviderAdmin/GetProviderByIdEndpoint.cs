using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;

/// <summary>
/// Endpoint responsável pela consulta de prestador específico por ID.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para consulta de prestador único
/// utilizando arquitetura CQRS. Permite que usuários consultem dados de
/// prestadores com autorização apropriada. Valida autorização antes de
/// retornar os dados do prestador.
/// </remarks>
[ExcludeFromCodeCoverage]
public class GetProviderByIdEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de consulta de prestador por ID.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint GET em "/{id:guid}" com:
    /// - Autorização por permissão (ProvidersRead)
    /// - Validação automática de GUID para o parâmetro ID
    /// - Documentação OpenAPI automática
    /// - Respostas estruturadas para sucesso (200) e não encontrado (404)
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/{id:guid}", GetProviderAsync)
            .WithName("GetProviderById")
            .WithSummary("Consultar prestador por ID")
            .WithDescription("""
                Recupera dados completos de um prestador de serviços através de seu identificador único.
                
                **Características:**
                - 🔍 Busca direta por ID único (GUID)
                - ⚡ Consulta otimizada com dados completos
                - 🔒 Controle de acesso: usuários autorizados
                - 📊 Retorna perfil completo do prestador
                
                **Resposta incluirá:**
                - Informações básicas do prestador
                - Perfil de negócio completo
                - Documentos associados
                - Qualificações e certificações
                - Status de verificação
                - Metadados de criação e atualização
                """)
            .RequirePermission(EPermission.ProvidersRead)
            .Produces<Response<ProviderDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    /// <summary>
    /// Implementa a lógica de consulta de prestador por ID.
    /// </summary>
    /// <param name="id">ID único do prestador (GUID)</param>
    /// <param name="queryDispatcher">Dispatcher para envio de queries CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Resultado HTTP com dados do prestador ou erro apropriado</returns>
    /// <remarks>
    /// Processo da consulta:
    /// 1. Valida ID do prestador no formato GUID
    /// 2. Cria query usando mapper ToQuery
    /// 3. Envia query através do dispatcher CQRS
    /// 4. Retorna resposta HTTP com dados do prestador ou NotFound
    /// </remarks>
    private static async Task<IResult> GetProviderAsync(
        Guid id,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = id.ToQuery();
        var result = await queryDispatcher.QueryAsync<GetProviderByIdQuery, Result<ProviderDto?>>(
            query, cancellationToken);

        return Handle(result);
    }
}
