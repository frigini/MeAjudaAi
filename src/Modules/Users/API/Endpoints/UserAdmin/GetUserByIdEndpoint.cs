using MeAjudaAi.Modules.Users.API.Mappers;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;

/// <summary>
/// Endpoint responsável pela consulta de usuário específico por ID.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para consulta de usuário único
/// utilizando arquitetura CQRS. Permite que usuários consultem seus próprios
/// dados ou administradores consultem dados de qualquer usuário. Valida
/// autorização antes de retornar os dados do usuário.
/// </remarks>
public class GetUserByIdEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de consulta de usuário por ID.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint GET em "/{id:guid}" com:
    /// - Autorização SelfOrAdmin (usuário pode ver próprios dados ou admin vê qualquer usuário)
    /// - Validação automática de GUID para o parâmetro ID
    /// - Documentação OpenAPI automática
    /// - Respostas estruturadas para sucesso (200) e não encontrado (404)
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet(ApiEndpoints.Users.GetById, GetUserAsync)
            .WithName("GetUser")
            .WithSummary("Consultar usuário por ID")
            .WithDescription("""
                Recupera dados completos de um usuário específico através de seu identificador único.
                
                **Características:**
                - 🔍 Busca direta por ID único (GUID)
                - ⚡ Consulta otimizada e cache automático
                - 🔒 Controle de acesso: usuário próprio ou administrador
                - 📊 Retorna dados completos do perfil
                
                **Resposta incluirá:**
                - Informações básicas do usuário
                - Dados do perfil (nome, sobrenome, email)
                - Metadados de criação e atualização
                - Papéis e permissões associados
                """)
            .RequireSelfOrAdmin()
            .Produces<Response<UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    /// <summary>
    /// Implementa a lógica de consulta de usuário por ID.
    /// </summary>
    /// <param name="id">ID único do usuário (GUID)</param>
    /// <param name="queryDispatcher">Dispatcher para envio de queries CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Resultado HTTP com dados do usuário ou erro apropriado</returns>
    /// <remarks>
    /// Processo da consulta:
    /// 1. Valida ID do usuário no formato GUID
    /// 2. Cria query usando mapper ToQuery
    /// 3. Envia query através do dispatcher CQRS
    /// 4. Retorna resposta HTTP com dados do usuário
    /// </remarks>
    private static async Task<IResult> GetUserAsync(
        Guid id,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = id.ToQuery();
        var result = await queryDispatcher.QueryAsync<GetUserByIdQuery, Result<UserDto>>(
            query, cancellationToken);

        return Handle(result);
    }
}
