using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;

/// <summary>
/// Endpoint responsável pela atualização do status de verificação de prestadores.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para gestão de status de verificação
/// utilizando arquitetura CQRS. Restrito apenas para administradores devido
/// à criticidade da operação. Controla elegibilidade para prestação de serviços.
/// </remarks>
public class UpdateVerificationStatusEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de atualização de status.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint PUT em "/{id:guid}/verification-status" com:
    /// - Autorização por permissão (ProvidersApprove)
    /// - Validação automática de GUID para o parâmetro ID
    /// - Documentação OpenAPI automática
    /// - Códigos de resposta apropriados
    /// - Nome único para referência
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/{id:guid}/verification-status", UpdateVerificationStatusAsync)
            .WithName("UpdateVerificationStatus")
            .WithSummary("Atualizar status de verificação")
            .WithDescription("""
                Atualiza o status de verificação de um prestador de serviços.
                
                **🔒 Acesso Restrito: Apenas Administradores**
                
                **Status disponíveis:**
                - **Pending**: Aguardando verificação
                - **Verified**: Verificado e aprovado para serviços
                - **Rejected**: Rejeitado na verificação
                - **Suspended**: Suspenso temporariamente
                
                **Características:**
                - ⚖️ Controle administrativo de elegibilidade
                - 📋 Auditoria automática de mudanças
                - 🔄 Atualização imediata do status
                - 📧 Notificações automáticas (futuro)
                
                **Efeitos do status:**
                - **Verified**: Prestador pode receber solicitações
                - **Pending**: Perfil visível mas sem solicitações
                - **Rejected**: Perfil oculto, requer correções
                - **Suspended**: Temporariamente inativo
                
                **Validações aplicadas:**
                - Prestador existente e ativo
                - Autorização administrativa
                - Transição de status válida
                - Documentação adequada para verificação
                """)
            .RequirePermission(EPermission.ProvidersApprove)
            .Produces<Response<ProviderDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

    /// <summary>
    /// Processa requisição de atualização de status de forma assíncrona.
    /// </summary>
    /// <param name="id">ID único do prestador</param>
    /// <param name="request">Dados do novo status de verificação</param>
    /// <param name="commandDispatcher">Dispatcher para envio de comandos CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado HTTP contendo:
    /// - 200 OK: Status atualizado com sucesso e dados atualizados do prestador
    /// - 400 Bad Request: Erro de validação ou atualização
    /// - 404 Not Found: Prestador não encontrado
    /// </returns>
    /// <remarks>
    /// Fluxo de execução:
    /// 1. Valida ID do prestador e autorização administrativa
    /// 2. Converte request em comando CQRS
    /// 3. Envia comando através do dispatcher
    /// 4. Processa resultado e retorna prestador atualizado
    /// 5. Registra evento de auditoria (futuro)
    /// </remarks>
    private static async Task<IResult> UpdateVerificationStatusAsync(
        Guid id,
        [FromBody] UpdateVerificationStatusRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return Results.BadRequest("Corpo da requisição é obrigatório");

        var command = request.ToCommand(id);
        var result = await commandDispatcher.SendAsync<UpdateVerificationStatusCommand, Result<ProviderDto>>(
            command, cancellationToken);

        return Handle(result);
    }
}
