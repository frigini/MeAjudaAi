using MeAjudaAi.Contracts.Modules.Communications;
using MeAjudaAi.Contracts.Modules.Communications.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Communications.API.Endpoints.Public;

public class GetCommunicationLogsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Communications.GetLogs, GetLogsAsync)
           .Produces(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status400BadRequest)
           .WithTags(CommunicationsEndpoints.Tag)
           .WithName("GetCommunicationLogs")
           .WithSummary("Obtém logs de comunicação")
           .WithDescription("Obtém a lista de logs de comunicações enviadas.");
    }

    /// <summary>
    /// Obtém os logs de comunicação com base nos filtros fornecidos.
    /// </summary>
    /// <param name="query">Parâmetros de consulta para filtragem de logs.</param>
    /// <param name="api">API do módulo de comunicações.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Uma lista de logs de comunicação.</returns>
    private static async Task<IResult> GetLogsAsync(
        [AsParameters] CommunicationLogQuery query,
        ICommunicationsModuleApi api,
        CancellationToken ct)
    {
        var result = await api.GetLogsAsync(query, ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
    }
}
