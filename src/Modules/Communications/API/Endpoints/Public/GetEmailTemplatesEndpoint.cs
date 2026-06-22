using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Contracts.Modules.Communications;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Communications.API.Endpoints.Public;

/// <summary>
/// Endpoint para busca de templates de e-mail.
/// </summary>
[ExcludeFromCodeCoverage]
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
        return result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToProblem();
    }
}
