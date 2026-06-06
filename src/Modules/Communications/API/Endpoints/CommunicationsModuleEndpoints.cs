using MeAjudaAi.Contracts.Modules.Communications;
using MeAjudaAi.Contracts.Modules.Communications.Queries;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.Communications.API.Endpoints;

public static class CommunicationsModuleEndpoints
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = BaseEndpoint.CreateVersionedGroup(endpoints, ApiEndpoints.Communications.Base, "Communications")
            .RequireAuthorization();

        group.MapGet(ApiEndpoints.Communications.GetLogs, async (
            [AsParameters] CommunicationLogQuery query,
            ICommunicationsModuleApi api,
            CancellationToken ct) =>
        {
            var result = await api.GetLogsAsync(query, ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).WithName("GetCommunicationLogs");

        group.MapGet(ApiEndpoints.Communications.GetTemplates, async (
            ICommunicationsModuleApi api,
            CancellationToken ct) =>
        {
            var result = await api.GetTemplatesAsync(ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).WithName("GetEmailTemplates");

        return endpoints;
    }
}
