using MeAjudaAi.Contracts.Modules.Communications;
using MeAjudaAi.Contracts.Modules.Communications.DTOs;
using MeAjudaAi.Contracts.Modules.Communications.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Communications.API.Endpoints.Public;

/// <summary>
/// Endpoint para busca dos logs de comunicação.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetCommunicationLogsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Communications.GetLogs, GetLogsAsync)
           .Produces<CommunicationLogDto[]>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status400BadRequest)
           .ProducesProblem(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
           .Produces(StatusCodes.Status401Unauthorized)
           .Produces(StatusCodes.Status403Forbidden)
           .ProducesProblem(StatusCodes.Status500InternalServerError)
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
        
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        var error = result.Error!;
        
        // Preserve status code if within valid HTTP error range (400-599), else fallback to 500
        var statusCode = (error.StatusCode >= 400 && error.StatusCode < 600) 
            ? error.StatusCode 
            : StatusCodes.Status500InternalServerError;

        return Results.Problem(detail: error.Message, statusCode: statusCode);
    }
}
