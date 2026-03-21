namespace MeAjudaAi.Web.Admin.Features.Theme;

/// <summary>
/// Actions do Fluxor para gerenciar o tema.
/// </summary>
public static class ThemeActions
{
    /// <summary>
    /// Action para alternar entre dark/light mode
    /// </summary>
    public record ToggleDarkModeAction;

    /// <summary>
    /// Action para definir dark mode explicitamente
    /// </summary>
    public record SetDarkModeAction(bool IsDarkMode);

    /// <summary>
    /// Action para alternar uso de preferência do sistema
    /// </summary>
    public record ToggleSystemPreferenceAction;
}
