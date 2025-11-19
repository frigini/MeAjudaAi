using MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;
using MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.ServiceCategory;
using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints;

/// <summary>
/// Classe responsável pelo mapeamento de todos os endpoints do módulo ServiceCatalogs.
/// </summary>
public static class ServiceCatalogsModuleEndpoints
{
    /// <summary>
    /// Mapeia todos os endpoints do módulo ServiceCatalogs.
    /// </summary>
    /// <param name="app">Aplicação web para configuração das rotas</param>
    public static void MapServiceCatalogsEndpoints(this WebApplication app)
    {
        // Service Categories endpoints
        var categoriesEndpoints = BaseEndpoint.CreateVersionedGroup(app, "catalogs/categories", "ServiceCategories");

        categoriesEndpoints.MapEndpoint<GetAllServiceCategoriesEndpoint>()
            .MapEndpoint<GetServiceCategoryByIdEndpoint>()
            .MapEndpoint<CreateServiceCategoryEndpoint>()
            .MapEndpoint<UpdateServiceCategoryEndpoint>()
            .MapEndpoint<ActivateServiceCategoryEndpoint>()
            .MapEndpoint<DeactivateServiceCategoryEndpoint>()
            .MapEndpoint<DeleteServiceCategoryEndpoint>();

        // Services endpoints
        var servicesEndpoints = BaseEndpoint.CreateVersionedGroup(app, "catalogs/services", "Services");

        servicesEndpoints.MapEndpoint<GetAllServicesEndpoint>()
            .MapEndpoint<GetServiceByIdEndpoint>()
            .MapEndpoint<GetServicesByCategoryEndpoint>()
            .MapEndpoint<CreateServiceEndpoint>()
            .MapEndpoint<UpdateServiceEndpoint>()
            .MapEndpoint<ChangeServiceCategoryEndpoint>()
            .MapEndpoint<ActivateServiceEndpoint>()
            .MapEndpoint<DeactivateServiceEndpoint>()
            .MapEndpoint<DeleteServiceEndpoint>()
            .MapEndpoint<ValidateServicesEndpoint>();
    }
}
