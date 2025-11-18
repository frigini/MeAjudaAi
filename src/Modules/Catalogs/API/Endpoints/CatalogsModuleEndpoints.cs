using MeAjudaAi.Shared.Endpoints;
using Microsoft.AspNetCore.Builder;

namespace MeAjudaAi.Modules.Catalogs.API.Endpoints;

/// <summary>
/// Classe responsável pelo mapeamento de todos os endpoints do módulo Catalogs.
/// </summary>
public static class CatalogsModuleEndpoints
{
    /// <summary>
    /// Mapeia todos os endpoints do módulo Catalogs.
    /// </summary>
    /// <param name="app">Aplicação web para configuração das rotas</param>
    public static void MapCatalogsEndpoints(this WebApplication app)
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
            .MapEndpoint<DeleteServiceEndpoint>();
    }
}
