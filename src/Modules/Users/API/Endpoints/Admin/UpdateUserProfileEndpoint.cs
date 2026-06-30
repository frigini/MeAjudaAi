using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Users.API.Mappers;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Users.API.Endpoints.Admin;

/// <summary>
/// Endpoint responsável pela atualização de perfil de usuários existentes.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para atualização de dados de perfil
/// utilizando arquitetura CQRS. Permite que usuários atualizem seus próprios
/// dados ou administradores atualizem dados de qualquer usuário. Valida
/// permissões e dados antes de processar a atualização.
/// </remarks>
[ExcludeFromCodeCoverage]
public class UpdateUserProfileEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de atualização de perfil.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint PUT em "/{id:guid}/profile" com:
    /// - Autorização SelfOrAdmin (usuário pode atualizar próprio perfil ou admin qualquer perfil)
    /// - Validação automática do formato GUID para ID
    /// - Documentação OpenAPI automática
    /// - Códigos de resposta apropriados
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPut(ApiEndpoints.Users.UpdateProfile, UpdateUserAsync)
            .WithName(ApiEndpoints.Users.Names.UpdateProfile)
            .WithSummary("Update user profile")
            .WithDescription("Updates profile information for an existing user")
            .RequireSelfOrAdmin()
            .Produces<Response<UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    /// <summary>
    /// Processa requisição de atualização de perfil de usuário de forma assíncrona.
    /// </summary>
    /// <param name="id">Identificador único do usuário a ser atualizado</param>
    /// <param name="request">Dados atualizados do perfil do usuário</param>
    /// <param name="commandDispatcher">Dispatcher para envio de comandos CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado HTTP contendo:
    /// - 200 OK: Perfil atualizado com sucesso e dados atualizados
    /// - 404 Not Found: Usuário não encontrado
    /// - 400 Bad Request: Erro de validação
    /// </returns>
    /// <remarks>
    /// Fluxo de execução:
    /// 1. Converte request em comando CQRS
    /// 2. Envia comando através do dispatcher (validação via pipeline)
    /// 3. Retorna resposta HTTP com dados atualizados
    ///
    /// Dados atualizáveis: FirstName, LastName, Email, PhoneNumber
    /// </remarks>
    private static async Task<IResult> UpdateUserAsync(
        Guid id,
        [FromBody] UpdateUserProfileRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand(id);
        var result = await commandDispatcher.SendAsync<UpdateUserProfileCommand, Result<UserDto>>(
            command, cancellationToken);

        return Handle(result);
    }
}
