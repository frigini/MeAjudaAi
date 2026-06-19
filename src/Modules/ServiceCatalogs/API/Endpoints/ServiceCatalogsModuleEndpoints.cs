using MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.Service;
using MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints.ServiceCategory;
using MeAjudaAi.Shared.Endpoints;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace MeAjudaAi.Modules.ServiceCatalogs.API.Endpoints;

public static class ServiceCatalogsModuleEndpoints
{
    public static void Map(IEndpointRouteBuilder app)
    {
        MapServiceCatalogsEndpoints((WebApplication)app);
    }

    public static void MapServiceCatalogsEndpoints(this WebApplication app)
    {
        // Service Categories endpoints
        var categoriesEndpoints = BaseEndpoint.CreateVersionedGroup(app, ApiEndpoints.ServiceCatalogs.Categories, "ServiceCategories");

        categoriesEndpoints.MapEndpoint<GetAllServiceCategoriesEndpoint>()
            .MapEndpoint<GetServiceCategoryByIdEndpoint>()
            .MapEndpoint<CreateServiceCategoryEndpoint>()
            .MapEndpoint<UpdateServiceCategoryEndpoint>()
            .MapEndpoint<ActivateServiceCategoryEndpoint>()
            .MapEndpoint<DeactivateServiceCategoryEndpoint>()
            .MapEndpoint<DeleteServiceCategoryEndpoint>();

        // Services endpoints
        var servicesEndpoints = BaseEndpoint.CreateVersionedGroup(app, ApiEndpoints.ServiceCatalogs.Services, "Services");

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
