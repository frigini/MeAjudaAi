using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.Queries;
using MeAjudaAi.Modules.Users.API.Mappers;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;

/// <summary>
/// Endpoint responsável pela consulta de usuário específico por email.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para consulta de usuário por email
/// utilizando arquitetura CQRS. Restrito apenas para administradores devido
/// à sensibilidade dos dados de email. Realiza busca direta no sistema
/// para localizar usuário através do endereço de email.
/// </remarks>
public class GetUserByEmailEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de consulta de usuário por email.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint GET em "/by-email/{email}" com:
    /// - Autorização AdminOnly (apenas administradores podem buscar por email)
    /// - Validação automática de formato de email
    /// - Documentação OpenAPI automática
    /// - Respostas estruturadas para sucesso (200) e não encontrado (404)
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapGet("/by-email/{email}", GetUserByEmailAsync)
            .WithName("GetUserByEmail")
            .WithSummary("Consultar usuário por email")
            .WithDescription("""
                Recupera dados completos de um usuário específico através de seu endereço de email.
                
                **Características:**
                - 🔍 Busca direta por endereço de email
                - ⚡ Consulta otimizada com índice de email
                - 🔒 Acesso restrito apenas para administradores
                - 📊 Retorna dados completos do perfil
                
                **Uso típico:**
                - Administradores verificando contas por email
                - Suporte técnico localizando usuários
                - Auditoria e investigações de segurança
                
                **Resposta incluirá:**
                - Informações básicas do usuário
                - Dados do perfil completo
                - Status da conta e metadados
                """)
            .RequireAdmin()
            .Produces<Response<UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    /// <summary>
    /// Implementa a lógica de consulta de usuário por email.
    /// </summary>
    /// <param name="email">Endereço de email do usuário</param>
    /// <param name="queryDispatcher">Dispatcher para envio de queries CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Resultado HTTP com dados do usuário ou erro apropriado</returns>
    /// <remarks>
    /// Processo da consulta:
    /// 1. Valida formato do email
    /// 2. Cria query usando mapper ToEmailQuery
    /// 3. Envia query através do dispatcher CQRS
    /// 4. Retorna resposta HTTP com dados do usuário
    /// </remarks>
    private static async Task<IResult> GetUserByEmailAsync(
        string email,
        IQueryDispatcher queryDispatcher,
        CancellationToken cancellationToken)
    {
        var query = email.ToEmailQuery();
        var result = await queryDispatcher.QueryAsync<GetUserByEmailQuery, Result<UserDto>>(
            query, cancellationToken);

        return Handle(result);
    }
}