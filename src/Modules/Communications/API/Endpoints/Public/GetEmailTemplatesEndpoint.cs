using MeAjudaAi.Contracts.Modules.Communications;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Communications.API.Endpoints.Public;

/// <summary>
/// Endpoint para busca de templates de e-mail.
/// </summary>
public class GetEmailTemplatesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Communications.GetTemplates, GetTemplatesAsync)
           .Produces(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status400BadRequest)
           .WithTags(CommunicationsEndpoints.Tag)
           .WithName("GetEmailTemplates")
           .WithSummary("Obtém templates de e-mail")
           .WithDescription("Obtém a lista de todos os templates de e-mail disponíveis.");
    }

    /// <summary>
    /// Obtém todos os templates de e-mail disponíveis no sistema.
    /// </summary>
    /// <param name="api">API do módulo de comunicações.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Uma lista de templates de e-mail.</returns>
    private static async Task<IResult> GetTemplatesAsync(
        ICommunicationsModuleApi api,
        CancellationToken ct)
    {
        var result = await api.GetTemplatesAsync(ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
    }
}
