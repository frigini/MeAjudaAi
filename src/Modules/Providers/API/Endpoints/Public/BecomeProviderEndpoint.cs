using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.Public;

[ExcludeFromCodeCoverage]
public class BecomeProviderEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("become", BecomeProviderAsync)
            .WithName(ApiEndpoints.Providers.Names.Become)
            .WithTags("Providers")
            .WithSummary("Tornar-se prestador (usuário já autenticado)")
            .WithDescription("Transforma o usuário autenticado em um prestador de serviços. Requer token de usuário.")
            .RequireAuthorization()
            .Produces<Response<ProviderDto>>(StatusCodes.Status201Created)
            .Produces<Response<object>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

    private static async Task<IResult> BecomeProviderAsync(
        HttpContext context,
        [FromBody] RegisterProviderApiRequest request,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var userIdString = GetUserId(context);
        if (!Guid.TryParse(userIdString, out var userId))
            return BadRequest("Formato de ID de usuário inválido");

        // Obter email do token (obrigatório). Não há fallback para o corpo da requisição.
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
        return Results.CreatedAtRoute(ApiEndpoints.Providers.Names.GetMyProfile, null, new Response<ProviderDto>(result.Value));
    }
}
