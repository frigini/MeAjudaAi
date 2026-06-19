using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Extensions;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;

/// <summary>
/// Endpoint responsável pela remoção de documentos de prestadores de serviços.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para remoção de documentos de verificação
/// utilizando arquitetura CQRS. Permite que prestadores removam seus próprios
/// documentos ou administradores gerenciem documentos. Operação irreversível.
/// </remarks>
public class RemoveDocumentEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de remoção de documento.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint DELETE em "/{id:guid}/documents/{documentType}" com:
    /// - Autorização por permissão (ProvidersUpdate)
    /// - Validação automática de GUID para o parâmetro ID
    /// - Validação automática de enum para documentType
    /// - Documentação OpenAPI automática
    /// - Códigos de resposta apropriados
    /// - Nome único para referência
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapDelete("/{id:guid}/documents/{documentType}", RemoveDocumentAsync)
            .WithName("RemoveDocument")
            .WithSummary("Remover documento do prestador")
            .WithDescription("""
                Remove um documento específico do perfil do prestador de serviços.
                
                **⚠️ Atenção: Esta operação é irreversível!**
                
                **Características:**
                - 🗑️ Remoção permanente de documento
                - 🔒 Controle de acesso: usuários com permissão ProvidersUpdate
                - ✅ Validação de existência do documento
                - 📋 Atualização automática do perfil
                
                **Tipos de documento removíveis:**
                - CPF/CNPJ
                - RG/Identidade
                - Comprovante de residência
                - Certificações profissionais
                - Outros documentos regulamentares
                
                **Validações aplicadas:**
                - Prestador existente e ativo
                - Documento existe no perfil
                - Autorização para remoção
                - Possível impacto no status de verificação
                
                **Efeitos colaterais:**
                - Pode alterar status de verificação do prestador
                - Remove histórico do documento
                - Pode impactar elegibilidade para serviços
                """)
            .RequirePermission(EPermission.ProvidersUpdate)
            .Produces<Response<ProviderDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

    /// <summary>
    /// Processa requisição de remoção de documento de forma assíncrona.
    /// </summary>
    /// <param name="id">ID único do prestador</param>
    /// <param name="documentType">Tipo do documento a ser removido</param>
    /// <param name="commandDispatcher">Dispatcher para envio de comandos CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado HTTP contendo:
    /// - 200 OK: Documento removido com sucesso e dados atualizados do prestador
    /// - 400 Bad Request: Erro de validação ou remoção
    /// - 404 Not Found: Prestador ou documento não encontrado
    /// </returns>
    /// <remarks>
    /// Fluxo de execução:
    /// 1. Valida ID do prestador e autorização
    /// 2. Cria comando usando mapper ToRemoveDocumentCommand
    /// 3. Envia comando através do dispatcher
    /// 4. Processa resultado e retorna prestador atualizado
    /// </remarks>
    private static async Task<IResult> RemoveDocumentAsync(
        Guid id,
        EDocumentType documentType,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = id.ToRemoveDocumentCommand(documentType);
        var result = await commandDispatcher.SendAsync<RemoveDocumentCommand, Result<ProviderDto>>(
            command, cancellationToken);

        return Handle(result);
    }
}
