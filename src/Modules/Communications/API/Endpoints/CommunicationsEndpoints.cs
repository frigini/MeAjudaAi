using MeAjudaAi.Modules.Communications.API.Endpoints.Public;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Communications.API.Endpoints;

[ExcludeFromCodeCoverage]
public static class CommunicationsEndpoints
{
    public const string Tag = "Communications";

    public static void Map(IEndpointRouteBuilder app)
    {
        var group = BaseEndpoint.CreateVersionedGroup(app, ApiEndpoints.Communications.Base, "Communications")
            .RequireAuthorization();

        group.MapEndpoint<GetCommunicationLogsEndpoint>()
             .MapEndpoint<GetEmailTemplatesEndpoint>();
    }
}
