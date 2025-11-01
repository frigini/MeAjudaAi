using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;

/// <summary>
/// Endpoint responsável pela exclusão lógica de prestadores de serviços.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para exclusão lógica (soft delete) de
/// prestadores utilizando arquitetura CQRS. Restrito apenas para administradores
/// devido à criticidade da operação. Preserva dados para auditoria e possível
/// recuperação futura.
/// </remarks>
public class DeleteProviderEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de exclusão de prestador.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint DELETE em "/{id:guid}" com:
    /// - Autorização AdminOnly (apenas administradores podem excluir prestadores)
    /// - Validação automática de GUID para o parâmetro ID
    /// - Documentação OpenAPI automática
    /// - Códigos de resposta apropriados
    /// - Nome único para referência
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/{id:guid}", DeleteProviderAsync)
            .WithName("DeleteProvider")
            .WithSummary("Excluir prestador de serviços")
            .WithDescription("""
                Realiza exclusão lógica (soft delete) de um prestador de serviços.
                
                **🔒 Acesso Restrito: Apenas Administradores**
                **⚠️ Operação Crítica: Requer justificativa administrativa**
                
                **Características:**
                - 🗑️ Exclusão lógica (dados preservados)
                - 🔒 Acesso restrito a administradores
                - 📋 Auditoria completa da operação
                - 🔄 Possibilidade de recuperação futura
                
                **Efeitos da exclusão:**
                - Prestador torna-se inativo imediatamente
                - Perfil removido das buscas públicas
                - Solicitações ativas são canceladas
                - Histórico preservado para auditoria
                - Usuário associado permanece ativo
                
                **Dados preservados:**
                - Informações completas do prestador
                - Histórico de documentos e verificações
                - Relacionamentos com solicitações
                - Metadados de criação e exclusão
                
                **Validações aplicadas:**
                - Prestador existente e ativo
                - Autorização administrativa
                - Não existem dependências críticas
                - Auditoria de motivo (futuro)
                """)
            .RequireAuthorization("AdminOnly")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

    /// <summary>
    /// Processa requisição de exclusão de prestador de forma assíncrona.
    /// </summary>
    /// <param name="id">ID único do prestador a ser excluído</param>
    /// <param name="commandDispatcher">Dispatcher para envio de comandos CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado HTTP contendo:
    /// - 204 No Content: Prestador excluído com sucesso
    /// - 400 Bad Request: Erro de validação ou exclusão
    /// - 404 Not Found: Prestador não encontrado
    /// </returns>
    /// <remarks>
    /// Fluxo de execução:
    /// 1. Valida ID do prestador e autorização administrativa
    /// 2. Cria comando usando mapper ToDeleteCommand
    /// 3. Envia comando através do dispatcher
    /// 4. Processa resultado e retorna status apropriado
    /// 5. Registra evento de auditoria (futuro)
    /// </remarks>
    private static async Task<IResult> DeleteProviderAsync(
        Guid id,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = id.ToDeleteCommand();
        var result = await commandDispatcher.SendAsync<DeleteProviderCommand, Result>(
            command, cancellationToken);

        return HandleNoContent(result);
    }
}
