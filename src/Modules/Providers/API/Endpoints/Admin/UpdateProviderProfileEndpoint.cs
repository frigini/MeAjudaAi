using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Shared.Authorization.Core.Enums;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.Admin;

/// <summary>
/// Endpoint responsável pela atualização de perfil de prestadores de serviços.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para atualização de dados de perfil
/// utilizando arquitetura CQRS. Permite que prestadores atualizem seus próprios
/// dados ou administradores atualizem dados de qualquer prestador. Valida
/// permissões e dados antes de processar a atualização.
/// </remarks>
[ExcludeFromCodeCoverage]
public class UpdateProviderProfileEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de atualização de perfil.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint PUT em "/{id:guid}" com:
    /// - Autorização por permissão (ProvidersUpdate)
    /// - Validação automática de GUID para o parâmetro ID
    /// - Documentação OpenAPI automática
    /// - Códigos de resposta apropriados
    /// - Nome único para referência
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut("/{id:guid}", UpdateProviderProfileAsync)
            .WithName(ApiEndpoints.Providers.Names.UpdateProfile)
            .WithSummary("Atualizar perfil do prestador")
            .WithDescription("""
                Atualiza informações do perfil de um prestador de serviços existente.
                
                **Características:**
                - ✏️ Atualização de dados pessoais/empresariais
                - 🏢 Modificação do perfil de negócio
                - 📞 Atualização de informações de contato
                - 📍 Alteração de endereço principal
                - 🔒 Controle de acesso: usuários com permissão ProvidersUpdate
                
                **Campos atualizáveis:**
                - Nome do prestador
                - Perfil de negócio completo
                - Informações de contato (email, telefone, website)
                - Endereço principal
                - Descrição do negócio
                
                **Validações aplicadas:**
                - Formato de email válido
                - Campos obrigatórios preenchidos
                - Autorização de acesso ao prestador
                """)
            .RequirePermission(EPermission.ProvidersUpdate)
            .Produces<Response<ProviderDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

    /// <summary>
    /// Processa requisição de atualização de perfil de forma assíncrona.
    /// </summary>
    /// <param name="id">ID único do prestador a ser atualizado</param>
    /// <param name="request">Dados atualizados do prestador</param>
    /// <param name="commandDispatcher">Dispatcher para envio de comandos CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado HTTP contendo:
    /// - 200 OK: Prestador atualizado com sucesso e dados atualizados
    /// - 400 Bad Request: Erro de validação ou atualização
    /// - 404 Not Found: Prestador não encontrado
    /// </returns>
    /// <remarks>
    /// Fluxo de execução:
    /// 1. Valida ID do prestador e autorização
    /// 2. Converte request em comando CQRS
    /// 3. Envia comando através do dispatcher
    /// 4. Processa resultado e retorna resposta HTTP apropriada
    /// </remarks>
    private static async Task<IResult> UpdateProviderProfileAsync(
        Guid id,
        [FromBody] UpdateProviderProfileRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        if (request is null)
            return Results.BadRequest("Corpo da requisição é obrigatório");

        var command = request.ToCommand(id);
        var result = await commandDispatcher.SendAsync<UpdateProviderProfileCommand, Result<ProviderDto>>(
            command, cancellationToken);

        return Handle(result);
    }
}
