using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.Admin;

/// <summary>
/// Endpoint responsável por solicitar correção de informações básicas de prestadores.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para retornar prestadores da etapa de verificação
/// de documentos para correção de informações básicas utilizando arquitetura CQRS.
/// Restrito a administradores e verificadores devido à criticidade da operação.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public class RequireBasicInfoCorrectionEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de solicitação de correção.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint POST definido em ApiEndpoints.Providers.RequireBasicInfoCorrection com:
    /// - Autorização por permissão (ProvidersApprove)
    /// - Validação automática de GUID para o parâmetro ID
    /// - Documentação OpenAPI automática
    /// - Códigos de resposta apropriados
    /// - Nome único para referência
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost(ApiEndpoints.Providers.RequireBasicInfoCorrection, RequireBasicInfoCorrectionAsync)
            .WithName(ApiEndpoints.Providers.Names.RequireBasicInfoCorrection)
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
                - 🔐 Identificação do solicitante extraída da autenticação
                
                **Fluxo após correção:**
                1. Prestador recebe notificação com motivo da correção
                2. Prestador atualiza informações básicas
                3. Prestador conclui informações básicas novamente
                4. Sistema retorna para verificação de documentos
                
                **Campos obrigatórios no request body:**
                - Reason: Motivo detalhado da correção necessária
                
                **Campos derivados do servidor:**
                - RequestedBy: Extraído automaticamente do contexto de autenticação (claims: name, sub ou email)
                
                **Validações aplicadas:**
                - Prestador em status PendingDocumentVerification
                - Motivo não pode ser vazio
                - Prestador existente e ativo
                - Autorização administrativa verificada
                - Identidade do solicitante autenticada
                """)
            .RequirePermission(EPermission.ProvidersApprove)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

    /// <summary>
    /// Processa requisição de solicitação de correção de forma assíncrona.
    /// </summary>
    /// <param name="id">ID único do prestador</param>
    /// <param name="request">Dados da solicitação de correção</param>
    /// <param name="commandDispatcher">Dispatcher para envio de comandos CQRS</param>
    /// <param name="httpContext">Contexto HTTP para obter o usuário autenticado</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado HTTP contendo:
    /// - 200 OK: Correção solicitada com sucesso
    /// - 400 Bad Request: Erro de validação ou solicitação
    /// - 401 Unauthorized: Usuário não autenticado
    /// - 404 Not Found: Prestador não encontrado
    /// </returns>
    /// <remarks>
    /// Fluxo de execução:
    /// 1. Valida ID do prestador e autorização
    /// 2. Extrai identidade do usuário autenticado do contexto HTTP
    /// 3. Converte request em comando CQRS com identidade verificada
    /// 4. Envia comando através do dispatcher
    /// 5. Processa resultado e retorna confirmação
    /// 6. Emite evento de domínio para notificação
    /// </remarks>
    private static async Task<IResult> RequireBasicInfoCorrectionAsync(
        Guid id,
        [FromBody] RequireBasicInfoCorrectionRequest request,
        ICommandDispatcher commandDispatcher,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return Results.BadRequest("Corpo da requisição é obrigatório");

        // Extrai a identidade do usuário autenticado do contexto HTTP
        var requestedBy = httpContext.User.Identity?.Name
            ?? httpContext.User.FindFirst("sub")?.Value
            ?? httpContext.User.FindFirst("email")?.Value
            ?? "system";

        var command = request.ToCommand(id, requestedBy);
        var result = await commandDispatcher.SendAsync<RequireBasicInfoCorrectionCommand, Result>(
            command, cancellationToken);

        return Handle(result);
    }
}
