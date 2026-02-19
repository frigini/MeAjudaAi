using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel.DataAnnotations;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.Public;

public class BecomeProviderEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("become", BecomeProviderAsync)
            .WithName("BecomeProvider")
            .WithTags("Providers")
            .WithSummary("Tornar-se prestador (usuário já autenticado)")
            .WithDescription("Transforma o usuário autenticado em um prestador de serviços. Requer token de usuário.")
            .RequireAuthorization()
            .Produces<Response<ProviderDto>>(StatusCodes.Status201Created)
            .Produces<Response<object>>(StatusCodes.Status400BadRequest);

    private static async Task<IResult> BecomeProviderAsync(
        HttpContext context,
        [FromBody] RegisterProviderApiRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var userIdString = GetUserId(context);
        if (!Guid.TryParse(userIdString, out var userId))
            return BadRequest("Formato de ID de usuário inválido");

        // Obter email do token se possível, senão usar do request
        var email = context.User?.FindFirst("email")?.Value;
        
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

        if (result.IsFailure)
            return Handle(result);
            
        // Retorna 201 Created com a localização do recurso (perfil do prestador)
        return Results.CreatedAtRoute("GetMyProviderProfile", null, new Response<ProviderDto>(result.Value!));
    }
}

public record RegisterProviderApiRequest(
    [Required, StringLength(100)] string Name,
    [Required, EnumDataType(typeof(EProviderType))] EProviderType Type,
    [Required, StringLength(20)] string DocumentNumber,
    [Phone, StringLength(20)] string? PhoneNumber,
    [EmailAddress] string? Email
);
