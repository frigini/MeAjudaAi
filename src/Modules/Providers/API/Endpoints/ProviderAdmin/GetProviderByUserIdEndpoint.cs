using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;

/// <summary>
/// Endpoint responsável pela consulta de prestador por ID do usuário.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para consulta de prestador através
/// do ID do usuário associado. Utiliza arquitetura CQRS e permite que
/// usuários consultem seus próprios dados de prestador ou administradores
/// consultem qualquer prestador pelo ID do usuário.
/// </remarks>
public class GetProviderByUserIdEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de consulta de prestador por ID do usuário.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint GET em "/by-user/{userId:guid}" com:
    /// - Autorização SelfOrAdmin (usuário pode ver próprios dados ou admin vê qualquer)
    /// - Validação automática de GUID para o parâmetro userId
    /// - Documentação OpenAPI automática
    /// - Respostas estruturadas para sucesso (200) e não encontrado (404)
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/by-user/{userId:guid}", GetProviderByUserAsync)
            .WithName("GetProviderByUserId")
            .WithSummary("Consultar prestador por ID do usuário")
            .WithDescription("""
                Recupera dados do prestador de serviços associado a um usuário específico.
                
                **Características:**
                - 👤 Busca por vinculação com usuário
                - ⚡ Consulta otimizada e direta
                - 🔒 Controle de acesso: próprio usuário ou administrador
                - 📊 Retorna dados completos do prestador
                
                **Casos de uso:**
                - Usuário consulta seu próprio perfil de prestador
                - Administrador consulta prestador de usuário específico
                - Verificação de existência de prestador para usuário
                
                **Resposta incluirá:**
                - Todos os dados do prestador de serviços
                - Perfil de negócio associado
                - Documentos e qualificações
                - Status de verificação atual
                """)
            .RequireAuthorization("SelfOrAdmin")
            .Produces<Response<ProviderDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    /// <summary>
    /// Implementa a lógica de consulta de prestador por ID do usuário.
    /// </summary>
    /// <param name="userId">ID único do usuário (GUID)</param>
    /// <param name="queryDispatcher">Dispatcher para envio de queries CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Resultado HTTP com dados do prestador ou erro apropriado</returns>
    /// <remarks>
    /// Processo da consulta:
    /// 1. Valida ID do usuário no formato GUID
    /// 2. Cria query usando mapper ToUserQuery
    /// 3. Envia query através do dispatcher CQRS
    /// 4. Retorna resposta HTTP com dados do prestador ou NotFound
    /// </remarks>
    private static async Task<IResult> GetProviderByUserAsync(
        Guid userId,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = userId.ToUserQuery();
        var result = await queryDispatcher.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
            query, cancellationToken);

        if (result.IsSuccess && result.Value == null)
            return NotFound("Provider not found for the specified user");

        return Handle(result);
    }
}
