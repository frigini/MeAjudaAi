using MeAjudaAi.Modules.Users.API.Mappers;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Contracts;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Functional;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Users.API.Endpoints.UserAdmin;

/// <summary>
/// Endpoint responsável pela criação de novos usuários no sistema.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para criação de usuários utilizando
/// arquitetura CQRS. Requer autorização de administrador e valida dados
/// antes de enviar comando para processamento. Integra com Keycloak para
/// gerenciamento de identidade.
/// </remarks>
public class CreateUserEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de criação de usuário.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint POST em "/" com:
    /// - Autorização obrigatória (AdminOnly)
    /// - Documentação OpenAPI automática
    /// - Códigos de resposta apropriados
    /// - Nome único para referência
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost(ApiEndpoints.Users.Create, CreateUserAsync)
            .WithName("CreateUser")
            .WithSummary("Create new user")
            .WithDescription("Creates a new user in the system with Keycloak integration")
            .Produces<Response<UserDto>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAdmin();

    /// <summary>
    /// Processa requisição de criação de usuário de forma assíncrona.
    /// </summary>
    /// <param name="request">Dados do usuário a ser criado</param>
    /// <param name="commandDispatcher">Dispatcher para envio de comandos CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado HTTP contendo:
    /// - 201 Created: Usuário criado com sucesso e dados do usuário
    /// - 400 Bad Request: Erro de validação ou criação
    /// </returns>
    /// <remarks>
    /// Fluxo de execução:
    /// 1. Converte request em comando CQRS
    /// 2. Envia comando através do dispatcher
    /// 3. Processa resultado e retorna resposta HTTP apropriada
    /// 4. Inclui localização do recurso criado no header
    /// </remarks>
    private static async Task<IResult> CreateUserAsync(
        [FromBody] CreateUserRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        var result = await commandDispatcher.SendAsync<CreateUserCommand, Result<UserDto>>(
            command, cancellationToken);

        return Handle(result, "CreateUser", new { id = result.Value?.Id });
    }
}
