using MeAjudaAi.Modules.Providers.API.Endpoints.ProviderAdmin;
using MeAjudaAi.Modules.Providers.API.Endpoints.ProviderServices;
using MeAjudaAi.Modules.Providers.API.Endpoints.Public;
using MeAjudaAi.Modules.Providers.API.Endpoints.Public.Me;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Contracts.Constants;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.Providers.API.Endpoints;

public static class ProvidersModuleEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        MapProvidersEndpoints(app);
    }

    public static void MapProvidersEndpoints(IEndpointRouteBuilder app)
    {
        // Usa o sistema unificado de versionamento via BaseEndpoint
        var endpoints = BaseEndpoint.CreateVersionedGroup(app, ApiEndpoints.Providers.Base, ModuleNames.Providers);
        // Remove .RequireAuthorization() global - cada endpoint define sua própria autorização

        // Endpoints de gestão de prestadores
        endpoints.MapEndpoint<GetProvidersEndpoint>()
            .MapEndpoint<CreateProviderEndpoint>()
            .MapEndpoint<GetProviderByIdEndpoint>()
            .MapEndpoint<GetPublicProviderByIdOrSlugEndpoint>() // Endpoint público por ID ou slug
            .MapEndpoint<BecomeProviderEndpoint>() // Endpoint para usuário autenticado virar prestador
            .MapEndpoint<GetMyProviderProfileEndpoint>()
            .MapEndpoint<UpdateMyProviderProfileEndpoint>()
            .MapEndpoint<GetProviderByUserIdEndpoint>()
            .MapEndpoint<GetProvidersByCityEndpoint>()
            .MapEndpoint<GetProvidersByStateEndpoint>()
            .MapEndpoint<GetProvidersByTypeEndpoint>()
            .MapEndpoint<GetProvidersByVerificationStatusEndpoint>()
            .MapEndpoint<UpdateProviderProfileEndpoint>()
            .MapEndpoint<AddDocumentEndpoint>()
            .MapEndpoint<RemoveDocumentEndpoint>()
            .MapEndpoint<UpdateVerificationStatusEndpoint>()
            .MapEndpoint<RequireBasicInfoCorrectionEndpoint>()
            .MapEndpoint<DeleteProviderEndpoint>()
            .MapEndpoint<UploadMyDocumentEndpoint>()
            .MapEndpoint<GetMyProviderStatusEndpoint>()
            .MapEndpoint<ActivateMyProviderProfileEndpoint>()
            .MapEndpoint<DeactivateMyProviderProfileEndpoint>()
            .MapEndpoint<ProviderVerificationEventsEndpoint>()
            .MapEndpoint<UpdateProviderDeviceTokenEndpoint>();
        
        // Endpoints de associação de serviços
        endpoints.MapEndpoint<AddServiceToProviderEndpoint>()
            .MapEndpoint<RemoveServiceFromProviderEndpoint>();
    }
}
