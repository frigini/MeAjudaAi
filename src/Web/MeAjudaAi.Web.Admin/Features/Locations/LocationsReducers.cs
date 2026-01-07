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
}
