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
/// Endpoint responsável pela adição de documentos a prestadores de serviços.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para adição de documentos de verificação
/// utilizando arquitetura CQRS. Permite que prestadores adicionem documentos
/// necessários para verificação ou administradores gerenciem documentos.
/// </remarks>
public class AddDocumentEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de adição de documento.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint POST em "/{id:guid}/documents" com:
    /// - Autorização por permissão (ProvidersUpdate)
    /// - Validação automática de GUID para o parâmetro ID
    /// - Documentação OpenAPI automática
    /// - Códigos de resposta apropriados
    /// - Nome único para referência
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("/{id:guid}/documents", AddDocumentAsync)
            .WithName("AddDocument")
            .WithSummary("Adicionar documento ao prestador")
            .WithDescription("""
                Adiciona um novo documento de verificação ao perfil do prestador de serviços.
                
                **Características:**
                - 📄 Adição de documentos de verificação
                - 🔒 Controle de acesso: usuários com permissão ProvidersUpdate
                - ✅ Validação automática de tipo e formato
                - 📋 Atualização automática do perfil
                
                **Tipos de documento suportados:**
                - CPF/CNPJ
                - RG/Identidade
                - Comprovante de residência
                - Certificações profissionais
                - Outros documentos regulamentares
                
                **Campos obrigatórios:**
                - Number: Número/código do documento
                - DocumentType: Tipo específico do documento
                
                **Validações aplicadas:**
                - Formato válido do número do documento
                - Tipo de documento permitido
                - Prestador existente e ativo
                - Não duplicação de documentos do mesmo tipo
                """)
            .RequirePermission(EPermission.ProvidersUpdate)
            .Produces<Response<ProviderDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

    /// <summary>
    /// Processa requisição de adição de documento de forma assíncrona.
    /// </summary>
    /// <param name="id">ID único do prestador</param>
    /// <param name="request">Dados do documento a ser adicionado</param>
    /// <param name="commandDispatcher">Dispatcher para envio de comandos CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado HTTP contendo:
    /// - 200 OK: Documento adicionado com sucesso e dados atualizados do prestador
    /// - 400 Bad Request: Erro de validação ou adição
    /// - 404 Not Found: Prestador não encontrado
    /// </returns>
    /// <remarks>
    /// Fluxo de execução:
    /// 1. Valida ID do prestador e autorização
    /// 2. Converte request em comando CQRS
    /// 3. Envia comando através do dispatcher
    /// 4. Processa resultado e retorna prestador atualizado
    /// </remarks>
    private static async Task<IResult> AddDocumentAsync(
        Guid id,
        [FromBody] AddDocumentRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return Results.BadRequest("Corpo da requisição é obrigatório");

        var command = request.ToCommand(id);
        var result = await commandDispatcher.SendAsync<AddDocumentCommand, Result<ProviderDto>>(
            command, cancellationToken);

        return Handle(result);
    }
}
