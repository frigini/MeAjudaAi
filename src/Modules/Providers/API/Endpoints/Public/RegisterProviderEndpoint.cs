using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.Public;

public class RegisterProviderEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("register", RegisterProviderAsync)
            .WithName("RegisterProvider")
            .WithTags("Providers")
            .WithSummary("Registrar novo prestador")
            .WithDescription("Cria um novo perfil de prestador para o usuário autenticado.")
            .RequireAuthorization()
            .Produces<Response<ProviderDto>>(StatusCodes.Status200OK)
            .Produces<Response<object>>(StatusCodes.Status400BadRequest);

    private static async Task<IResult> RegisterProviderAsync(
        HttpContext context,
        [FromBody] RegisterProviderRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var userIdString = GetUserId(context);
        if (!Guid.TryParse(userIdString, out var userId))
            return BadRequest("Formato de ID de usuário inválido");

        // Obter email do token se possível, senão usar do request
        var email = context.User?.FindFirst("email")?.Value;
        
        if (string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(request.Email))
            email = request.Email;

        if (string.IsNullOrEmpty(email))
            return BadRequest("Email é obrigatório e não foi encontrado no token.");

        var command = new RegisterProviderCommand(
            userId,
            request.Name,
            email,
            request.PhoneNumber,
            request.Type,
            request.DocumentNumber
        );

        var result = await commandDispatcher.SendAsync<RegisterProviderCommand, Result<ProviderDto>>(
            command, cancellationToken);
            
        // Se retornar sucesso, pode ser criado novo ou existente.
        // Retornamos 200 OK com os dados.
        return Handle(result);
    }
}

public record RegisterProviderRequest(
    string Name,
    EProviderType Type,
    string DocumentNumber,
    string? PhoneNumber,
    string? Email
);
