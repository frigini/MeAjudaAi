using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Users.Application.Commands;
using MeAjudaAi.Modules.Users.Application.DTOs;
using MeAjudaAi.Modules.Users.Application.DTOs.Requests;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Users.API.Endpoints.Public;

/// <summary>
/// Endpoint público responsável pelo registro de novos clientes no sistema.
/// </summary>
/// <remarks>
/// Implementa padrão de endpoint mínimo para auto-registro de clientes
/// utilizando arquitetura CQRS. Endpoint público que permite que novos
/// usuários criem suas contas com papel de cliente.
/// </remarks>
[ExcludeFromCodeCoverage]
public class RegisterCustomerEndpoint : BaseEndpoint, IEndpoint
{
    /// <summary>
    /// Configura o mapeamento do endpoint de registro de cliente.
    /// </summary>
    /// <param name="app">Builder de rotas do endpoint</param>
    /// <remarks>
    /// Configura endpoint POST em "/register" com:
    /// - Acesso público (AllowAnonymous)
    /// - Documentação OpenAPI automática
    /// - Códigos de resposta apropriados
    /// - Nome único para referência
    /// </remarks>
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("register", RegisterCustomerAsync)
            .WithName(ApiEndpoints.Users.Names.Register)
            .WithTags("Users")
            .WithSummary("Registrar novo cliente")
            .WithDescription("Cria uma nova conta de usuário com papel de cliente.")
            .AllowAnonymous();

    /// <summary>
    /// Processa requisição de registro de cliente de forma assíncrona.
    /// </summary>
    /// <param name="request">Dados do cliente a ser registrado</param>
    /// <param name="commandDispatcher">Dispatcher para envio de comandos CQRS</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>
    /// Resultado HTTP contendo:
    /// - 201 Created: Cliente registrado com sucesso e dados do usuário
    /// - 400 Bad Request: Erro de validação ou registro
    /// </returns>
    /// <remarks>
    /// Fluxo de execução:
    /// 1. Converte request em comando CQRS
    /// 2. Envia comando através do dispatcher
    /// 3. Processa resultado e retorna resposta HTTP apropriada
    /// 4. Inclui localização do recurso criado no header
    /// </remarks>
    private static async Task<IResult> RegisterCustomerAsync(
        RegisterCustomerRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCustomerCommand(
            request.Name,
            request.Email,
            request.Password,
            request.PhoneNumber,
            request.TermsAccepted,
            request.AcceptedPrivacyPolicy
        );

        var result = await commandDispatcher.SendAsync<RegisterCustomerCommand, Result<UserDto>>(
            command, cancellationToken);

        return Handle(result);
    }
}
