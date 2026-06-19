using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Communications.Application.Commands;
using MeAjudaAi.Shared.Commands;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Communications.API.Endpoints.Admin;

/// <summary>
/// Endpoint para criação de um novo template de e-mail.
/// </summary>
[ExcludeFromCodeCoverage]
public class CreateEmailTemplateEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Communications.CreateTemplate, HandleAsync)
           .Produces<Guid>(StatusCodes.Status201Created)
           .ProducesProblem(StatusCodes.Status400BadRequest)
           .WithTags(CommunicationsEndpoints.Tag)
           .WithName("CreateEmailTemplate")
           .WithSummary("Cria um template de e-mail")
           .WithDescription("Cria um novo template de e-mail no sistema.");
    }


    /// <summary>
    /// Manipula a requisição de criação de um novo template de e-mail.
    /// </summary>
    /// <param name="command">Comando de criação do template.</param>
    /// <param name="dispatcher">Dispensador de comandos.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>O ID do template criado.</returns>
    private static async Task<IResult> HandleAsync(
        [FromBody] CreateEmailTemplateCommand command,
        [FromServices] ICommandDispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.SendAsync<CreateEmailTemplateCommand, Result<Guid>>(command, ct);
        return result.IsSuccess 
            ? Results.Created($"/api/v1/{ApiEndpoints.Communications.Base}/templates/{result.Value}", result.Value) 
            : result.Error.ToProblem();
    }
}
