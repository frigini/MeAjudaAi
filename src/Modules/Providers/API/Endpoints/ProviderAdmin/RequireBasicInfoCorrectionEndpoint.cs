using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;

/// <summary>
/// Endpoint responsável por solicitar correção de informações básicas de prestadores.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para retornar prestadores da etapa de verificação
/// de documentos para correção de informações básicas utilizando arquitetura CQRS.
/// Restrito a administradores e verificadores devido à criticidade da operação.
/// </remarks>
public class RequireBasicInfoCorrectionEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de solicitação de correção.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint POST em "/{id:guid}/require-basic-info-correction" com:
    /// - Autorização AdminOnly (apenas administradores/verificadores podem solicitar correções)
    /// - Validação automática de GUID para o parâmetro ID
    /// - Documentação OpenAPI automática
    /// - Códigos de resposta apropriados
    /// - Nome único para referência
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id:guid}/require-basic-info-correction", RequireBasicInfoCorrectionAsync)
            .WithName("RequireBasicInfoCorrection")
            .WithSummary("Solicitar correção de informações básicas")
            .WithDescription("""
                Retorna um prestador de serviços para correção de informações básicas
                durante o processo de verificação de documentos.
                
                **🔒 Acesso Restrito: Apenas Administradores/Verificadores**
                
                **Quando usar:**
                - Informações básicas incorretas ou incompletas
                - Inconsistências identificadas durante verificação de documentos
                - Dados empresariais que precisam ser atualizados
                - Informações de contato inválidas
                
                **Características:**
                - 🔄 Retorna prestador para status PendingBasicInfo
                - 📧 Notificação automática ao prestador (futuro)
                - 📋 Auditoria completa da solicitação
                - ⚖️ Motivo obrigatório para rastreabilidade
                
                **Fluxo após correção:**
                1. Prestador recebe notificação com motivo da correção
                2. Prestador atualiza informações básicas
                3. Prestador conclui informações básicas novamente
                4. Sistema retorna para verificação de documentos
                
                **Campos obrigatórios:**
                - Reason: Motivo detalhado da correção necessária
                - RequestedBy: Identificador do verificador/administrador
                
                **Validações aplicadas:**
                - Prestador em status PendingDocumentVerification
                - Motivo não pode ser vazio
                - Prestador existente e ativo
                - Autorização administrativa
                """)
            .RequireAuthorization("AdminOnly")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

    /// <summary>
    /// Processa requisição de solicitação de correção de forma assíncrona.
    /// </summary>
    /// <param name="id">ID único do prestador</param>
    /// <param name="request">Dados da solicitação de correção</param>
    /// <param name="commandDispatcher">Dispatcher para envio de comandos CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado HTTP contendo:
    /// - 200 OK: Correção solicitada com sucesso
    /// - 400 Bad Request: Erro de validação ou solicitação
    /// - 404 Not Found: Prestador não encontrado
    /// </returns>
    /// <remarks>
    /// Fluxo de execução:
    /// 1. Valida ID do prestador e autorização
    /// 2. Converte request em comando CQRS
    /// 3. Envia comando através do dispatcher
    /// 4. Processa resultado e retorna confirmação
    /// 5. Emite evento de domínio para notificação
    /// </remarks>
    private static async Task<IResult> RequireBasicInfoCorrectionAsync(
        Guid id,
        [FromBody] RequireBasicInfoCorrectionRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return Results.BadRequest("Request body is required");

        var command = request.ToCommand(id);
        var result = await commandDispatcher.SendAsync<RequireBasicInfoCorrectionCommand, Result>(
            command, cancellationToken);

        return Handle(result);
    }
}
