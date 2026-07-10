using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Modules.Communications.API.Endpoints.Admin;
using MeAjudaAi.Modules.Communications.API.Endpoints.Public;
using MeAjudaAi.Shared.Authorization.Core.Enums;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Endpoints;
using System.Diagnostics.CodeAnalysis;

namespace MeAjudaAi.Modules.Communications.API.Endpoints;

/// <summary>
/// Classe responsável pelo mapeamento de todos os endpoints do módulo Communications.
/// </summary>
[ExcludeFromCodeCoverage]
public static class CommunicationsEndpoints
{
    public const string Tag = "Communications";

    /// <summary>
    /// Mapeia todos os endpoints do módulo Communications.
    /// </summary>
    /// <param name="app">Aplicação web para configuração das rotas</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        var group = BaseEndpoint.CreateVersionedGroup(app, ApiEndpoints.Communications.Base, Tag);

        group.MapEndpoint<GetCommunicationLogsEndpoint>()
            .MapEndpoint<GetEmailTemplatesEndpoint>()
            .RequirePermission(EPermission.CommunicationsRead);

        group.MapEndpoint<CreateEmailTemplateEndpoint>()
            .MapEndpoint<UpdateEmailTemplateEndpoint>()
            .MapEndpoint<ActivateEmailTemplateEndpoint>()
            .MapEndpoint<DeactivateEmailTemplateEndpoint>()
            .RequirePermission(EPermission.CommunicationsManage);
    }
}
