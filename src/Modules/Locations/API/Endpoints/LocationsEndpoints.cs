using MeAjudaAi.Modules.Locations.API.Endpoints.Admin;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Extensions;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.Locations.API.Endpoints;

/// <summary>
/// Classe responsável pelo mapeamento de todos os endpoints do módulo Locations.
/// </summary>
/// <remarks>
/// Utiliza o sistema unificado de versionamento via BaseEndpoint e organiza
/// todos os endpoints relacionados a cidades permitidas (Allowed Cities) em um grupo
/// versionado com autorização global aplicada.
/// </remarks>
public static class LocationsEndpoints
{
    public const string Tag = "Locations";

    /// <summary>
    /// Mapeia todos os endpoints do módulo Locations.
    /// </summary>
    /// <param name="app">Aplicação web para configuração das rotas</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        // Usa o sistema unificado de versionamento via BaseEndpoint
        // RequirePermission aplicado no grupo garante que todos endpoints são protegidos por padrão
        var endpoints = BaseEndpoint.CreateVersionedGroup(app, ApiEndpoints.Locations.AdminAllowedCities, "Allowed Cities")
            .RequirePermission(EPermission.LocationsManage);

        // Endpoints de gestão de cidades permitidas (Admin only)
        endpoints.MapEndpoint<CreateAllowedCityEndpoint>()
            .MapEndpoint<GetAllAllowedCitiesEndpoint>()
            .MapEndpoint<GetAllowedCitiesByStateEndpoint>()
            .MapEndpoint<GetAllowedCityByIdEndpoint>()
            .MapEndpoint<UpdateAllowedCityEndpoint>()
            .MapEndpoint<PatchAllowedCityEndpoint>()
            .MapEndpoint<DeleteAllowedCityEndpoint>();

        // Endpoints gerais de localizações (interações auxiliares)
        // Grupo: /api/v1/locations
        var locationEndpoints = BaseEndpoint.CreateVersionedGroup(app, ApiEndpoints.Locations.Base, Tag)
            .RequirePermission(EPermission.LocationsManage);

        locationEndpoints.MapEndpoint<SearchLocationsEndpoint>();
    }
}
