using MeAjudaAi.Modules.Locations.API.Endpoints.LocationsAdmin;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Extensions;

namespace MeAjudaAi.Modules.Locations.API.Endpoints;

/// <summary>
/// Classe responsável pelo mapeamento de todos os endpoints do módulo Locations.
/// </summary>
/// <remarks>
/// Utiliza o sistema unificado de versionamento via BaseEndpoint e organiza
/// todos os endpoints relacionados a cidades permitidas (Allowed Cities) em um grupo
/// versionado com autorização global aplicada.
/// </remarks>
public static class LocationsModuleEndpoints
{
    /// <summary>
    /// Mapeia todos os endpoints do módulo Locations.
    /// </summary>
    /// <param name="app">Aplicação web para configuração das rotas</param>
    public static void MapLocationsEndpoints(this WebApplication app)
    {
        // Usa o sistema unificado de versionamento via BaseEndpoint
        // RequirePermission aplicado no grupo garante que todos endpoints são protegidos por padrão
        var endpoints = BaseEndpoint.CreateVersionedGroup(app, ApiEndpoints.Locations.AdminAllowedCities, "Allowed Cities")
            .RequirePermission(EPermission.LocationsManage);

        // Endpoints de gestão de cidades permitidas (Admin only)
        endpoints.MapEndpoint<CreateAllowedCityEndpoint>()
            .MapEndpoint<GetAllAllowedCitiesEndpoint>()
            .MapEndpoint<GetAllowedCityByIdEndpoint>()
            .MapEndpoint<UpdateAllowedCityEndpoint>()
            .MapEndpoint<PatchAllowedCityEndpoint>()
            .MapEndpoint<DeleteAllowedCityEndpoint>();

        // Endpoints gerais de localizações (interações auxiliares)
        // Grupo: /api/v1/locations
        var locationEndpoints = BaseEndpoint.CreateVersionedGroup(app, ApiEndpoints.Locations.Base, "Locations")
            .RequirePermission(EPermission.LocationsManage);

        locationEndpoints.MapEndpoint<SearchLocationsEndpoint>();
    }
}
