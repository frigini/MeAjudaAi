using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Communications.Application.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Communications.API.Endpoints.Public;

/// <summary>
/// Endpoint para ativação de um template de e-mail.
/// </summary>
public class ActivateEmailTemplateEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPatch(ApiEndpoints.Communications.ActivateTemplate, HandleAsync)
           .Produces(StatusCodes.Status204NoContent)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .WithTags(CommunicationsEndpoints.Tag)
           .WithName("ActivateEmailTemplate")
           .WithSummary("Ativa um template de e-mail")
           .WithDescription("Torna um template de e-mail ativo para uso.");
    }

    /// <summary>
    /// Manipula a requisição de ativação de um template de e-mail.
    /// </summary>
    /// <param name="id">ID do template.</param>
    /// <param name="dispatcher">Dispensador de comandos.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Resultado da operação.</returns>
    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync<SetEmailTemplateStatusCommand, Result>(new SetEmailTemplateStatusCommand(id, true, Guid.NewGuid()), ct);
        return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
    }
}
