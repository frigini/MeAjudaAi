using MeAjudaAi.Modules.Providers.API.Mappers;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs.Requests;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Providers.API.Endpoints.Public.Me;

/// <summary>
/// Endpoint para o próprio prestador fazer upload de documentos.
/// Requer autenticação com role provider-*.
/// </summary>
/// <remarks>
/// Versão self-service do <see cref="ProviderAdmin.AddDocumentEndpoint"/> (admin-only).
/// Reutiliza o mesmo <see cref="AddDocumentCommand"/> e <see cref="AddDocumentRequest"/>.
/// </remarks>
// TODO: Enforce "ProviderPolicy" or specific roles when authorization policies are defined globally.
// Currently allows any authenticated user, but logic verifies if they have a Provider profile.
public class UploadMyDocumentEndpoint : BaseEndpoint, IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
        => app.MapPost("me/documents", UploadMyDocumentAsync)
            .WithName("UploadMyDocument")
            .WithTags("Providers - Me")
            .WithSummary("Upload de documento pelo próprio prestador")
            .WithDescription("Permite que o prestador adicione documentos ao seu próprio perfil.")
            .RequireAuthorization()
            .Produces<Response<ProviderDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

    private static async Task<IResult> UploadMyDocumentAsync(
        HttpContext context,
        [FromBody] AddDocumentRequest request,
        IQueryDispatcher queryDispatcher,
        ICommandDispatcher commandDispatcher,
        CancellationToken cancellationToken)
    {
        var userIdString = GetUserId(context);
        if (!Guid.TryParse(userIdString, out var userId))
            return BadRequest("Formato de ID de usuário inválido");

        // Busca o provider do usuário autenticado
        var query = new GetProviderByUserIdQuery(userId);
        var providerResult = await queryDispatcher.QueryAsync<GetProviderByUserIdQuery, Result<ProviderDto?>>(
            query, cancellationToken);

        if (providerResult.IsFailure)
        {
            // Retorna mensagem genérica para não expor interna
            return BadRequest("Não foi possível validar o perfil do prestador. Tente novamente.");
        }

        if (providerResult.Value is null)
            return NotFound("Perfil do prestador não encontrado para o usuário atual.");

        // Reutiliza AddDocumentCommand — mesma lógica do endpoint admin
        var command = request.ToCommand(providerResult.Value.Id);
        var result = await commandDispatcher.SendAsync<AddDocumentCommand, Result<ProviderDto>>(
            command, cancellationToken);

        return Handle(result);
    }
}
