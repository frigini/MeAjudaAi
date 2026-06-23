using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Communications.Application.Commands;
using MeAjudaAi.Modules.Communications.Application.DTOs;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;

using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Communications.API.Endpoints.Admin;

/// <summary>
/// Endpoint para atualização de um template de e-mail.
/// </summary>
[ExcludeFromCodeCoverage]
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
        
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return result.Error.ToProblem();
    }
}
