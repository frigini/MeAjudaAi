using MeAjudaAi.Modules.Users.API.Mappers;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;

/// <summary>
/// Endpoint responsável pela exclusão de usuários do sistema.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para exclusão lógica de usuários
/// utilizando arquitetura CQRS. Restrito apenas para administradores devido
/// à criticidade da operação. Realiza soft delete preservando dados para
/// auditoria e possível recuperação futura.
/// </remarks>
public class DeleteUserEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de exclusão de usuário.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint DELETE em "/{id:guid}" com:
    /// - Autorização AdminOnly (apenas administradores podem excluir usuários)
    /// - Validação automática de GUID para o parâmetro ID
    /// - Soft delete preservando dados para auditoria
    /// - Resposta 204 No Content para sucesso
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete(ApiEndpoints.Users.Delete, DeleteUserAsync)
            .WithName("DeleteUser")
            .WithSummary("Excluir usuário")
            .WithDescription("""
                Realiza exclusão lógica (soft delete) de um usuário específico no sistema.
                
                **Características:**
                - 🗑️ Soft delete preservando dados para auditoria
                - ⚡ Operação otimizada e transacional
                - 🔒 Acesso restrito apenas para administradores
                - 📊 Logs completos de auditoria
                
                **Importante:**
                - Usuário será marcado como inativo, não removido fisicamente
                - Dados preservados para conformidade e auditoria
                - Operação irreversível através da API (requer intervenção manual)
                - Sessions ativas do usuário serão invalidadas
                
                **Resposta:**
                - 204 No Content: Exclusão realizada com sucesso
                - 404 Not Found: Usuário não encontrado
                """)
            .RequireAdmin()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

    /// <summary>
    /// Implementa a lógica de exclusão de usuário.
    /// </summary>
    /// <param name="id">ID único do usuário a ser excluído (GUID)</param>
    /// <param name="commandDispatcher">Dispatcher para envio de commands CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Resultado HTTP sem conteúdo (204) ou erro apropriado</returns>
    /// <remarks>
    /// Processo de exclusão:
    /// 1. Valida ID do usuário no formato GUID
    /// 2. Cria command usando mapper ToDeleteCommand
    /// 3. Envia command através do dispatcher CQRS
    /// 4. Retorna resposta HTTP 204 No Content
    /// </remarks>
    private static async Task<IResult> DeleteUserAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = id.ToDeleteCommand();
        var result = await commandDispatcher.SendAsync<DeleteUserCommand, Result>(
            command, cancellationToken);

        return HandleNoContent(result);
    }
}
