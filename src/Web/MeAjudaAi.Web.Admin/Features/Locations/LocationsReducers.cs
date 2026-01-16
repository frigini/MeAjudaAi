using Fluxor;
using static MeAjudaAi.Web.Admin.Features.Locations.LocationsActions;

namespace MeAjudaAi.Web.Admin.Features.Locations;

/// <summary>
/// Reducers Fluxor para gerenciamento de estado de cidades permitidas.
/// Métodos puros que transformam o estado baseado em actions.
/// </summary>
public static class LocationsReducers
{
    // --- Load Reducers ---

    [ReducerMethod]
    public static LocationsState ReduceLoadAllowedCitiesAction(LocationsState state, LoadAllowedCitiesAction _)
        => state with { IsLoading = true, ErrorMessage = null };

    [ReducerMethod]
    public static LocationsState ReduceLoadAllowedCitiesSuccessAction(LocationsState state, LoadAllowedCitiesSuccessAction action)
        => state with
        {
            AllowedCities = action.Cities,
            IsLoading = false,
            ErrorMessage = null
        };

    [ReducerMethod]
    public static LocationsState ReduceLoadAllowedCitiesFailureAction(LocationsState state, LoadAllowedCitiesFailureAction action)
        => state with
        {
            IsLoading = false,
            ErrorMessage = action.ErrorMessage
        };

    // --- Create Reducer ---

    [ReducerMethod]
    public static LocationsState ReduceAddAllowedCityAction(LocationsState state, AddAllowedCityAction action)
        => state with
        {
            AllowedCities = [.. state.AllowedCities, action.City]
        };

    // --- Update Reducer ---

    [ReducerMethod]
    public static LocationsState ReduceUpdateAllowedCityAction(LocationsState state, UpdateAllowedCityAction action)
        => state with
        {
            AllowedCities = state.AllowedCities
                .Select(c => c.Id == action.CityId ? action.UpdatedCity : c)
                .ToList()
        };

    // --- Delete Reducer ---

    [ReducerMethod]
    public static LocationsState ReduceRemoveAllowedCityAction(LocationsState state, RemoveAllowedCityAction action)
        => state with
        {
            AllowedCities = state.AllowedCities.Where(c => c.Id != action.CityId).ToList()
        };

    // --- Toggle Active Reducer ---

    [ReducerMethod]
    public static LocationsState ReduceUpdateCityActiveStatusAction(LocationsState state, UpdateCityActiveStatusAction action)
        => state with
        {
            AllowedCities = state.AllowedCities
                .Select(c => c.Id == action.CityId ? c with { IsActive = action.IsActive } : c)
                .ToList()
        };

    // --- Error Reducer ---

    [ReducerMethod]
    public static LocationsState ReduceClearErrorAction(LocationsState state, ClearErrorAction _)
        => state with { ErrorMessage = null };

    // ========== DELETE OPERATIONS ==========

    [ReducerMethod]
    public static LocationsState ReduceDeleteAllowedCityAction(LocationsState state, DeleteAllowedCityAction action)
        => state with
        {
            IsDeletingCity = true,
            DeletingCityId = action.CityId,
            ErrorMessage = null
        };

    [ReducerMethod]
    public static LocationsState ReduceDeleteAllowedCitySuccessAction(LocationsState state, DeleteAllowedCitySuccessAction _)
        => state with
        {
            IsDeletingCity = false,
            DeletingCityId = null,
            ErrorMessage = null
        };

    [ReducerMethod]
    public static LocationsState ReduceDeleteAllowedCityFailureAction(LocationsState state, DeleteAllowedCityFailureAction action)
        => state with
        {
            IsDeletingCity = false,
            DeletingCityId = null,
            ErrorMessage = action.ErrorMessage
        };

    // ========== TOGGLE OPERATIONS ==========

    [ReducerMethod]
    public static LocationsState ReduceToggleCityActivationAction(LocationsState state, ToggleCityActivationAction action)
        => state with
        {
            IsTogglingCity = true,
            TogglingCityId = action.CityId,
            ErrorMessage = null
        };

    [ReducerMethod]
    public static LocationsState ReduceToggleCityActivationSuccessAction(LocationsState state, ToggleCityActivationSuccessAction _)
        => state with
        {
            IsTogglingCity = false,
            TogglingCityId = null,
            ErrorMessage = null
        };

    [ReducerMethod]
    public static LocationsState ReduceToggleCityActivationFailureAction(LocationsState state, ToggleCityActivationFailureAction action)
        => state with
        {
            IsTogglingCity = false,
            TogglingCityId = null,
            ErrorMessage = action.ErrorMessage
        };
}
