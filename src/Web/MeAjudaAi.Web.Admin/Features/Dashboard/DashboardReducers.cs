using Fluxor;
using static MeAjudaAi.Web.Admin.Features.Dashboard.DashboardActions;

namespace MeAjudaAi.Web.Admin.Features.Dashboard;

/// <summary>
/// Reducers do Fluxor para a feature de Dashboard.
/// Funções puras que recebem o estado atual e uma ação, retornando o novo estado.
/// </summary>
public static class DashboardReducers
{
    /// <summary>
    /// Reducer para LoadDashboardStatsAction - marca IsLoading=true e limpa erros
    /// </summary>
    [ReducerMethod]
    public static DashboardState OnLoadDashboardStats(DashboardState state, LoadDashboardStatsAction action)
    {
        return state with 
        { 
            IsLoading = true,
            ErrorMessage = null
        };
    }

    /// <summary>
    /// Reducer para LoadDashboardStatsSuccessAction - popula KPIs e marca IsLoading=false
    /// </summary>
    [ReducerMethod]
    public static DashboardState OnLoadDashboardStatsSuccess(
        DashboardState state, 
        LoadDashboardStatsSuccessAction action)
    {
        return state with 
        { 
            TotalProviders = action.TotalProviders,
            PendingVerifications = action.PendingVerifications,
            ActiveServices = action.ActiveServices,
            IsLoading = false,
            ErrorMessage = null
        };
    }

    /// <summary>
    /// Reducer para LoadDashboardStatsFailureAction - marca erro e IsLoading=false
    /// </summary>
    [ReducerMethod]
    public static DashboardState OnLoadDashboardStatsFailure(
        DashboardState state, 
        LoadDashboardStatsFailureAction action)
    {
        return state with 
        { 
            IsLoading = false,
            ErrorMessage = action.ErrorMessage
        };
    }
}
