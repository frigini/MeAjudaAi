using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Communications.Application.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Mvc;

using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Communications.API.Endpoints.Admin;

/// <summary>
/// Endpoint para desativação de um template de e-mail.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeactivateEmailTemplateEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPatch(ApiEndpoints.Communications.DeactivateTemplate, HandleAsync)
           .Produces(StatusCodes.Status204NoContent)
           .ProducesProblem(StatusCodes.Status400BadRequest)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .WithTags(CommunicationsEndpoints.Tag)
           .WithName("DeactivateEmailTemplate")
           .WithSummary("Desativa um template de e-mail")
           .WithDescription("Torna um template de e-mail inativo.");
    }

    /// <summary>
    /// Manipula a requisição de desativação de um template de e-mail.
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
        var result = await dispatcher.SendAsync<SetEmailTemplateStatusCommand, Result>(new SetEmailTemplateStatusCommand(id, false, Guid.NewGuid()), ct);
        
        if (result.IsSuccess)
        {
            return Results.NoContent();
        }

        return result.Error.ToProblem();
    }
}
