using Fluxor;

namespace MeAjudaAi.Web.Admin.Features.Modules.ServiceCatalogs;

/// <summary>
/// Reducers para estado de catálogo de serviços
/// </summary>
public static class ServiceCatalogsReducers
{
    // ========== CATEGORIES ==========

    [ReducerMethod]
    public static ServiceCatalogsState ReduceLoadCategoriesAction(ServiceCatalogsState state, ServiceCatalogsActions.LoadCategoriesAction _)
        => state with { IsLoadingCategories = true, ErrorMessage = null };

    [ReducerMethod]
    public static ServiceCatalogsState ReduceLoadCategoriesSuccessAction(ServiceCatalogsState state, ServiceCatalogsActions.LoadCategoriesSuccessAction action)
        => state with { Categories = action.Categories, IsLoadingCategories = false };

    [ReducerMethod]
    public static ServiceCatalogsState ReduceLoadCategoriesFailureAction(ServiceCatalogsState state, ServiceCatalogsActions.LoadCategoriesFailureAction action)
        => state with { IsLoadingCategories = false, ErrorMessage = action.ErrorMessage };

    [ReducerMethod]
    public static ServiceCatalogsState ReduceAddCategoryAction(ServiceCatalogsState state, ServiceCatalogsActions.AddCategoryAction action)
        => state with { Categories = [..state.Categories, action.Category] };

    [ReducerMethod]
    public static ServiceCatalogsState ReduceRemoveCategoryAction(ServiceCatalogsState state, ServiceCatalogsActions.RemoveCategoryAction action)
        => state with { Categories = state.Categories.Where(c => c.Id != action.CategoryId).ToList() };

    [ReducerMethod]
    public static ServiceCatalogsState ReduceUpdateCategoryAction(ServiceCatalogsState state, ServiceCatalogsActions.UpdateCategoryAction action)
    {
        var updated = state.Categories.Select(c =>
            c.Id == action.CategoryId
                ? c with { Name = action.Name, Description = action.Description, DisplayOrder = action.DisplayOrder }
                : c
        ).ToList();
        return state with { Categories = updated };
    }

    [ReducerMethod]
    public static ServiceCatalogsState ReduceUpdateCategoryActiveStatusAction(ServiceCatalogsState state, ServiceCatalogsActions.UpdateCategoryActiveStatusAction action)
    {
        var updated = state.Categories.Select(c =>
            c.Id == action.CategoryId
                ? c with { IsActive = action.IsActive }
                : c
        ).ToList();
        return state with { Categories = updated };
    }

    // ========== SERVICES ==========

    [ReducerMethod]
    public static ServiceCatalogsState ReduceLoadServicesAction(ServiceCatalogsState state, ServiceCatalogsActions.LoadServicesAction _)
        => state with { IsLoadingServices = true, ErrorMessage = null };

    [ReducerMethod]
    public static ServiceCatalogsState ReduceLoadServicesSuccessAction(ServiceCatalogsState state, ServiceCatalogsActions.LoadServicesSuccessAction action)
        => state with { Services = action.Services, IsLoadingServices = false };

    [ReducerMethod]
    public static ServiceCatalogsState ReduceLoadServicesFailureAction(ServiceCatalogsState state, ServiceCatalogsActions.LoadServicesFailureAction action)
        => state with { IsLoadingServices = false, ErrorMessage = action.ErrorMessage };

    [ReducerMethod]
    public static ServiceCatalogsState ReduceAddServiceAction(ServiceCatalogsState state, ServiceCatalogsActions.AddServiceAction action)
        => state with { Services = [..state.Services, action.Service] };

    [ReducerMethod]
    public static ServiceCatalogsState ReduceRemoveServiceAction(ServiceCatalogsState state, ServiceCatalogsActions.RemoveServiceAction action)
        => state with { Services = state.Services.Where(s => s.Id != action.ServiceId).ToList() };

    [ReducerMethod]
    public static ServiceCatalogsState ReduceUpdateServiceAction(ServiceCatalogsState state, ServiceCatalogsActions.UpdateServiceAction action)
    {
        var updated = state.Services.Select(s =>
            s.Id == action.ServiceId
                ? s with { Name = action.Name }
                : s
        ).ToList();
        return state with { Services = updated };
    }

    [ReducerMethod]
    public static ServiceCatalogsState ReduceUpdateServiceActiveStatusAction(ServiceCatalogsState state, ServiceCatalogsActions.UpdateServiceActiveStatusAction action)
    {
        var updated = state.Services.Select(s =>
            s.Id == action.ServiceId
                ? s with { IsActive = action.IsActive }
                : s
        ).ToList();
        return state with { Services = updated };
    }

    // ========== COMMON ==========

    [ReducerMethod]
    public static ServiceCatalogsState ReduceClearErrorAction(ServiceCatalogsState state, ServiceCatalogsActions.ClearErrorAction _)
        => state with { ErrorMessage = null };

    // ========== CATEGORY DELETE OPERATIONS ==========

    [ReducerMethod]
    public static ServiceCatalogsState ReduceDeleteCategoryAction(ServiceCatalogsState state, ServiceCatalogsActions.DeleteCategoryAction action)
        => state with
        {
            IsDeletingCategory = true,
            DeletingCategoryId = action.CategoryId,
            ErrorMessage = null
        };

    [ReducerMethod]
    public static ServiceCatalogsState ReduceDeleteCategorySuccessAction(ServiceCatalogsState state, ServiceCatalogsActions.DeleteCategorySuccessAction _)
        => state with
        {
            IsDeletingCategory = false,
            DeletingCategoryId = null,
            ErrorMessage = null
        };

    [ReducerMethod]
    public static ServiceCatalogsState ReduceDeleteCategoryFailureAction(ServiceCatalogsState state, ServiceCatalogsActions.DeleteCategoryFailureAction action)
        => state with
        {
            IsDeletingCategory = false,
            DeletingCategoryId = null,
            ErrorMessage = action.ErrorMessage
        };

    // ========== CATEGORY TOGGLE OPERATIONS ==========

    [ReducerMethod]
    public static ServiceCatalogsState ReduceToggleCategoryActivationAction(ServiceCatalogsState state, ServiceCatalogsActions.ToggleCategoryActivationAction action)
        => state with
        {
            IsTogglingCategory = true,
            TogglingCategoryId = action.CategoryId,
            ErrorMessage = null
        };

    [ReducerMethod]
    public static ServiceCatalogsState ReduceToggleCategoryActivationSuccessAction(ServiceCatalogsState state, ServiceCatalogsActions.ToggleCategoryActivationSuccessAction _)
        => state with
        {
            IsTogglingCategory = false,
            TogglingCategoryId = null,
            ErrorMessage = null
        };

    [ReducerMethod]
    public static ServiceCatalogsState ReduceToggleCategoryActivationFailureAction(ServiceCatalogsState state, ServiceCatalogsActions.ToggleCategoryActivationFailureAction action)
        => state with
        {
            IsTogglingCategory = false,
            TogglingCategoryId = null,
            ErrorMessage = action.ErrorMessage
        };

    // ========== SERVICE DELETE OPERATIONS ==========

    [ReducerMethod]
    public static ServiceCatalogsState ReduceDeleteServiceAction(ServiceCatalogsState state, ServiceCatalogsActions.DeleteServiceAction action)
        => state with
        {
            IsDeletingService = true,
            DeletingServiceId = action.ServiceId,
            ErrorMessage = null
        };

    [ReducerMethod]
    public static ServiceCatalogsState ReduceDeleteServiceSuccessAction(ServiceCatalogsState state, ServiceCatalogsActions.DeleteServiceSuccessAction _)
        => state with
        {
            IsDeletingService = false,
            DeletingServiceId = null,
            ErrorMessage = null
        };

    [ReducerMethod]
    public static ServiceCatalogsState ReduceDeleteServiceFailureAction(ServiceCatalogsState state, ServiceCatalogsActions.DeleteServiceFailureAction action)
        => state with
        {
            IsDeletingService = false,
            DeletingServiceId = null,
            ErrorMessage = action.ErrorMessage
        };

    // ========== SERVICE TOGGLE OPERATIONS ==========

    [ReducerMethod]
    public static ServiceCatalogsState ReduceToggleServiceActivationAction(ServiceCatalogsState state, ServiceCatalogsActions.ToggleServiceActivationAction action)
        => state with
        {
            IsTogglingService = true,
            TogglingServiceId = action.ServiceId,
            ErrorMessage = null
        };

    [ReducerMethod]
    public static ServiceCatalogsState ReduceToggleServiceActivationSuccessAction(ServiceCatalogsState state, ServiceCatalogsActions.ToggleServiceActivationSuccessAction _)
        => state with
        {
            IsTogglingService = false,
            TogglingServiceId = null,
            ErrorMessage = null
        };

    [ReducerMethod]
    public static ServiceCatalogsState ReduceToggleServiceActivationFailureAction(ServiceCatalogsState state, ServiceCatalogsActions.ToggleServiceActivationFailureAction action)
        => state with
        {
            IsTogglingService = false,
            TogglingServiceId = null,
            ErrorMessage = action.ErrorMessage
        };
}
