using Fluxor;
using MeAjudaAi.Client.Contracts.Api;
using MeAjudaAi.Web.Admin.Extensions;
using MudBlazor;

namespace MeAjudaAi.Web.Admin.Features.Modules.ServiceCatalogs;

/// <summary>
/// Effects para operações assíncronas de catálogo de serviços
/// </summary>
public sealed class ServiceCatalogsEffects
{
    private readonly IServiceCatalogsApi _serviceCatalogsApi;
    private readonly ISnackbar _snackbar;

    public ServiceCatalogsEffects(IServiceCatalogsApi serviceCatalogsApi, ISnackbar snackbar)
    {
        _serviceCatalogsApi = serviceCatalogsApi;
        _snackbar = snackbar;
    }

    /// <summary>
    /// Effect para carregar categorias
    /// </summary>
    [EffectMethod]
    public async Task HandleLoadCategoriesAction(ServiceCatalogsActions.LoadCategoriesAction action, IDispatcher dispatcher)
    {
        try
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
        catch (Exception ex)
        {
            var errorMessage = $"Erro ao carregar categorias: {ex.Message}";
            dispatcher.Dispatch(new ServiceCatalogsActions.LoadCategoriesFailureAction(errorMessage));
        }
    }

    /// <summary>
    /// Effect para carregar serviços
    /// </summary>
    [EffectMethod]
    public async Task HandleLoadServicesAction(ServiceCatalogsActions.LoadServicesAction action, IDispatcher dispatcher)
    {
        try
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
        catch (Exception ex)
        {
            var errorMessage = $"Erro ao carregar serviços: {ex.Message}";
            dispatcher.Dispatch(new ServiceCatalogsActions.LoadServicesFailureAction(errorMessage));
        }
    }

    /// <summary>
    /// Effect para atualizar categoria
    /// </summary>
    [EffectMethod]
    public async Task HandleUpdateCategoryAction(ServiceCatalogsActions.UpdateCategoryAction action, IDispatcher dispatcher)
    {
        var request = new MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs.UpdateServiceCatalogCategoryRequestDto(
            action.Name,
            action.Description,
            action.DisplayOrder);

        await _snackbar.ExecuteApiCallAsync(
            apiCall: () => _serviceCatalogsApi.UpdateCategoryAsync(action.CategoryId, request),
            operationName: "Atualizar categoria",
            onSuccess: _ =>
            {
                _snackbar.Add("Categoria atualizada com sucesso!", Severity.Success);
                dispatcher.Dispatch(new ServiceCatalogsActions.LoadCategoriesAction());
            },
            onError: ex =>
            {
                // Error handled by snackbar
            });
    }

    /// <summary>
    /// Effect para excluir categoria
    /// </summary>
    [EffectMethod]
    public async Task HandleDeleteCategoryAction(ServiceCatalogsActions.DeleteCategoryAction action, IDispatcher dispatcher)
    {
        await _snackbar.ExecuteApiCallAsync(
            apiCall: () => _serviceCatalogsApi.DeleteCategoryAsync(action.CategoryId),
            operationName: "Excluir categoria",
            onSuccess: _ =>
            {
                dispatcher.Dispatch(new ServiceCatalogsActions.DeleteCategorySuccessAction(action.CategoryId));
                _snackbar.Add("Categoria excluída com sucesso!", Severity.Success);
                dispatcher.Dispatch(new ServiceCatalogsActions.RemoveCategoryAction(action.CategoryId));
            },
            onError: ex =>
            {
                dispatcher.Dispatch(new ServiceCatalogsActions.DeleteCategoryFailureAction(action.CategoryId, ex.Message));
            });
    }

    /// <summary>
    /// Effect para alternar ativação de categoria
    /// </summary>
    [EffectMethod]
    public async Task HandleToggleCategoryActivationAction(ServiceCatalogsActions.ToggleCategoryActivationAction action, IDispatcher dispatcher)
    {
        await _snackbar.ExecuteApiCallAsync(
            apiCall: () => action.Activate
                ? _serviceCatalogsApi.ActivateCategoryAsync(action.CategoryId)
                : _serviceCatalogsApi.DeactivateCategoryAsync(action.CategoryId),
            operationName: action.Activate ? "Ativar categoria" : "Desativar categoria",
            onSuccess: _ =>
            {
                dispatcher.Dispatch(new ServiceCatalogsActions.ToggleCategoryActivationSuccessAction(action.CategoryId, action.Activate));
                _snackbar.Add($"Categoria {(action.Activate ? "ativada" : "desativada")} com sucesso!", Severity.Success);
                dispatcher.Dispatch(new ServiceCatalogsActions.UpdateCategoryActiveStatusAction(action.CategoryId, action.Activate));
            },
            onError: ex =>
            {
                dispatcher.Dispatch(new ServiceCatalogsActions.ToggleCategoryActivationFailureAction(action.CategoryId, ex.Message));
            });
    }

    /// <summary>
    /// Effect para atualizar serviço
    /// </summary>
    [EffectMethod]
    public async Task HandleUpdateServiceAction(ServiceCatalogsActions.UpdateServiceAction action, IDispatcher dispatcher)
    {
        var request = new MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs.UpdateServiceRequestDto(
            Name: action.Name,
            Description: action.Description
        );

        await _snackbar.ExecuteApiCallAsync(
            apiCall: () => _serviceCatalogsApi.UpdateServiceAsync(action.ServiceId, request),
            operationName: "Atualizar serviço",
            onSuccess: _ =>
            {
                _snackbar.Add("Serviço atualizado com sucesso!", Severity.Success);
                dispatcher.Dispatch(new ServiceCatalogsActions.LoadServicesAction());
            },
            onError: ex =>
            {
                // Error handled by snackbar
            });
    }

    /// <summary>
    /// Effect para excluir serviço
    /// </summary>
    [EffectMethod]
    public async Task HandleDeleteServiceAction(ServiceCatalogsActions.DeleteServiceAction action, IDispatcher dispatcher)
    {
        await _snackbar.ExecuteApiCallAsync(
            apiCall: () => _serviceCatalogsApi.DeleteServiceAsync(action.ServiceId),
            operationName: "Excluir serviço",
            onSuccess: _ =>
            {
                dispatcher.Dispatch(new ServiceCatalogsActions.DeleteServiceSuccessAction(action.ServiceId));
                _snackbar.Add("Serviço excluído com sucesso!", Severity.Success);
                dispatcher.Dispatch(new ServiceCatalogsActions.RemoveServiceAction(action.ServiceId));
            },
            onError: ex =>
            {
                dispatcher.Dispatch(new ServiceCatalogsActions.DeleteServiceFailureAction(action.ServiceId, ex.Message));
            });
    }

    /// <summary>
    /// Effect para alternar ativação de serviço
    /// </summary>
    [EffectMethod]
    public async Task HandleToggleServiceActivationAction(ServiceCatalogsActions.ToggleServiceActivationAction action, IDispatcher dispatcher)
    {
        await _snackbar.ExecuteApiCallAsync(
            apiCall: () => action.Activate
                ? _serviceCatalogsApi.ActivateServiceAsync(action.ServiceId)
                : _serviceCatalogsApi.DeactivateServiceAsync(action.ServiceId),
            operationName: action.Activate ? "Ativar serviço" : "Desativar serviço",
            onSuccess: _ =>
            {
                dispatcher.Dispatch(new ServiceCatalogsActions.ToggleServiceActivationSuccessAction(action.ServiceId, action.Activate));
                _snackbar.Add($"Serviço {(action.Activate ? "ativado" : "desativado")} com sucesso!", Severity.Success);
                dispatcher.Dispatch(new ServiceCatalogsActions.UpdateServiceActiveStatusAction(action.ServiceId, action.Activate));
            },
            onError: ex =>
            {
                dispatcher.Dispatch(new ServiceCatalogsActions.ToggleServiceActivationFailureAction(action.ServiceId, ex.Message));
            });
    }
}
