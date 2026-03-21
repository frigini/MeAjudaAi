using Fluxor;
using static MeAjudaAi.Web.Admin.Features.Theme.ThemeActions;

namespace MeAjudaAi.Web.Admin.Features.Theme;

/// <summary>
/// Reducers do Fluxor para gerenciar o tema.
/// </summary>
public static class ThemeReducers
{
    /// <summary>
    /// Reducer para alternar dark mode
    /// </summary>
    [ReducerMethod]
    public static ThemeState ReduceToggleDarkModeAction(ThemeState state, ToggleDarkModeAction action)
    {
        return state with 
        { 
            IsDarkMode = !state.IsDarkMode,
            UseSystemPreference = false // Desabilita preferência do sistema ao alternar manualmente
        };
    }

    /// <summary>
    /// Reducer para definir dark mode explicitamente
    /// </summary>
    [ReducerMethod]
    public static ThemeState ReduceSetDarkModeAction(ThemeState state, SetDarkModeAction action)
    {
        return state with 
        { 
            IsDarkMode = action.IsDarkMode 
        };
    }

    /// <summary>
    /// Reducer para alternar uso de preferência do sistema
    /// </summary>
    [ReducerMethod]
    public static ThemeState ReduceToggleSystemPreferenceAction(ThemeState state, ToggleSystemPreferenceAction action)
    {
        return state with 
        { 
            UseSystemPreference = !state.UseSystemPreference 
        };
    }
}
