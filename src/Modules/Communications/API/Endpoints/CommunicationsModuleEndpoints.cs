using MeAjudaAi.Contracts.Modules.Communications;
using MeAjudaAi.Contracts.Modules.Communications.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Communications.API.Endpoints;

public static class CommunicationsModuleEndpoints
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/communications")
            .WithTags("Communications")
            .RequireAuthorization();

        group.MapGet("/logs", async (
            [AsParameters] CommunicationLogQuery query,
            ICommunicationsModuleApi api,
            CancellationToken ct) =>
        {
            var result = await api.GetLogsAsync(query, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).WithName("GetCommunicationLogs");

        group.MapGet("/templates", async (
            ICommunicationsModuleApi api,
            CancellationToken ct) =>
        {
            var result = await api.GetTemplatesAsync(ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).WithName("GetEmailTemplates");

        return endpoints;
    }
}
