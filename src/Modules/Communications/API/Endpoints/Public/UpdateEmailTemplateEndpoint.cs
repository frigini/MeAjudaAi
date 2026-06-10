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
/// Endpoint para atualização de um template de e-mail.
/// </summary>
public class UpdateEmailTemplateEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut(ApiEndpoints.Communications.UpdateTemplate, HandleAsync)
           .Produces(StatusCodes.Status204NoContent)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .WithTags(CommunicationsEndpoints.Tag)
           .WithName("UpdateEmailTemplate")
           .WithSummary("Atualiza um template de e-mail")
           .WithDescription("Atualiza o conteúdo de um template de e-mail existente.");
    }

    /// <summary>
    /// Manipula a requisição de atualização de um template de e-mail existente.
    /// </summary>
    /// <param name="id">ID do template.</param>
    /// <param name="body">Conteúdo da atualização.</param>
    /// <param name="dispatcher">Dispensador de comandos.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Resultado da operação.</returns>
    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateEmailTemplateBody body,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync<UpdateEmailTemplateCommand, Result>(new UpdateEmailTemplateCommand(id, body.Subject, body.HtmlBody, body.TextBody, Guid.NewGuid()), ct);
        return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
    }
}

public sealed record UpdateEmailTemplateBody(string Subject, string HtmlBody, string TextBody);
