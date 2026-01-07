using Fluxor;
using MeAjudaAi.Client.Contracts.Api;

namespace MeAjudaAi.Web.Admin.Features.ServiceCatalogs;

/// <summary>
/// Effects para operações assíncronas de catálogo de serviços
/// </summary>
public sealed class ServiceCatalogsEffects
{
    private readonly IServiceCatalogsApi _serviceCatalogsApi;

    public ServiceCatalogsEffects(IServiceCatalogsApi serviceCatalogsApi)
    {
        _serviceCatalogsApi = serviceCatalogsApi;
    }

    /// <summary>
    /// Effect para carregar categorias
    /// </summary>
    [EffectMethod]
    public async Task HandleLoadCategoriesAction(ServiceCatalogsActions.LoadCategoriesAction action, IDispatcher dispatcher)
    {
        var result = await _serviceCatalogsApi.GetAllServiceCategoriesAsync(action.ActiveOnly);

        if (result.IsSuccess && result.Value != null)
        {
            dispatcher.Dispatch(new ServiceCatalogsActions.LoadCategoriesSuccessAction(result.Value));
        }
        else
        {
            var errorMessage = result.Error?.Message ?? "Erro ao carregar categorias";
            dispatcher.Dispatch(new ServiceCatalogsActions.LoadCategoriesFailureAction(errorMessage));
        }
    }

    /// <summary>
    /// Effect para carregar serviços
    /// </summary>
    [EffectMethod]
    public async Task HandleLoadServicesAction(ServiceCatalogsActions.LoadServicesAction action, IDispatcher dispatcher)
    {
        var result = await _serviceCatalogsApi.GetAllServicesAsync(action.ActiveOnly);

        if (result.IsSuccess && result.Value != null)
        {
            dispatcher.Dispatch(new ServiceCatalogsActions.LoadServicesSuccessAction(result.Value));
        }
        else
        {
            var errorMessage = result.Error?.Message ?? "Erro ao carregar serviços";
            dispatcher.Dispatch(new ServiceCatalogsActions.LoadServicesFailureAction(errorMessage));
        }
    }
}
